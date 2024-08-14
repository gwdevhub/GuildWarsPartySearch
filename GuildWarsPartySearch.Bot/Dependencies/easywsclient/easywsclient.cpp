#include <fcntl.h>
#include <vector>
#include <string>
#include <cstdint>
#include <cstdarg>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <sys/types.h>

#ifdef _WIN32
    #if defined(_MSC_VER) && !defined(_CRT_SECURE_NO_WARNINGS)
        #define _CRT_SECURE_NO_WARNINGS
    #endif
    #ifndef WIN32_LEAN_AND_MEAN
        #define WIN32_LEAN_AND_MEAN
    #endif
    #include <WinSock2.h>
    #include <WS2tcpip.h>
    #pragma comment(lib, "ws2_32")
    #include <io.h>
    #ifndef _SSIZE_T_DEFINED
        typedef int ssize_t;
        #define _SSIZE_T_DEFINED
    #endif
    #ifndef _SOCKET_T_DEFINED
        typedef SOCKET socket_t;
        #define _SOCKET_T_DEFINED
    #endif
    #ifndef snprintf
        #define snprintf _snprintf_s
    #endif
    #define socketerrno WSAGetLastError()
    #define SOCKET_EAGAIN_EINPROGRESS WSAEINPROGRESS
    #define SOCKET_EWOULDBLOCK WSAEWOULDBLOCK
#else
    #include <netdb.h>
    #include <netinet/tcp.h>
    #include <sys/socket.h>
    #include <sys/time.h>
    #include <unistd.h>
    #ifndef _SOCKET_T_DEFINED
        typedef int socket_t;
        #define _SOCKET_T_DEFINED
    #endif
    #ifndef INVALID_SOCKET
        #define INVALID_SOCKET (-1)
    #endif
    #ifndef SOCKET_ERROR
        #define SOCKET_ERROR   (-1)
    #endif
    #define closesocket(s) ::close(s)
    #include <errno.h>
    #define socketerrno errno
    #define SOCKET_EAGAIN_EINPROGRESS EAGAIN
    #define SOCKET_EWOULDBLOCK EWOULDBLOCK
#endif

#include <mbedtls/ssl.h>
#include <mbedtls/net_sockets.h>
#include <mbedtls/entropy.h>
#include <mbedtls/ctr_drbg.h>
#include <mbedtls/x509_crt.h>
#include <mbedtls/error.h>
#include <mbedtls/debug.h>

#include "easywsclient.hpp"

namespace { // private module-only namespace

    struct ConnectionContext {
        socket_t sockfd;
        mbedtls_ssl_context sslHandle;
        mbedtls_ssl_config sslConfig;
        mbedtls_ctr_drbg_context ctr_drbg;
        mbedtls_entropy_context entropy;
        bool is_ssl = false;
    };

    int log_error(const char* format, ...) {
        va_list vl;
        va_start(vl, format);
        int written = vfprintf(stderr, format, vl);
        va_end(vl);
        return written;
    }

    socket_t hostname_connect(const std::string& hostname, int port) {
        struct addrinfo hints;
        struct addrinfo* result;
        struct addrinfo* p;
        int ret;
        socket_t sockfd = INVALID_SOCKET;
        char sport[16];
        memset(&hints, 0, sizeof(hints));
        hints.ai_family = AF_UNSPEC;
        hints.ai_socktype = SOCK_STREAM;
        snprintf(sport, 16, "%d", port);
        if ((ret = getaddrinfo(hostname.c_str(), sport, &hints, &result)) != 0) {
            log_error("%d\n", ret);
            log_error("getaddrinfo: %s\n", gai_strerror(ret));
            return INVALID_SOCKET;
        }
        for (p = result; p != NULL; p = p->ai_next) {
            sockfd = socket(p->ai_family, p->ai_socktype, p->ai_protocol);
            if (sockfd == INVALID_SOCKET) { continue; }
            if (connect(sockfd, p->ai_addr, p->ai_addrlen) != SOCKET_ERROR) {
                break;
            }
            closesocket(sockfd);
            sockfd = INVALID_SOCKET;
        }
        freeaddrinfo(result);
        return sockfd;
    }

    int kWrite(ConnectionContext* ptConnCtx, const char* buf, int len, int flags) {
        if (!ptConnCtx->is_ssl) {
            return ::send(ptConnCtx->sockfd, buf, len, flags);
        }
        else {
            return mbedtls_ssl_write(&ptConnCtx->sslHandle, (const unsigned char*)buf, len);
        }
    }

    int kRead(ConnectionContext* ptConnCtx, char* buf, int len, int flags) {
        if (!ptConnCtx->is_ssl) {
            return ::recv(ptConnCtx->sockfd, buf, len, flags);
        }
        else {
            return mbedtls_ssl_read(&ptConnCtx->sslHandle, (unsigned char*)buf, len);
        }
    }

    void klose(ConnectionContext* ptConnCtx) {
        if (ptConnCtx->sockfd != INVALID_SOCKET) {
            closesocket(ptConnCtx->sockfd);
        }
        mbedtls_ssl_free(&ptConnCtx->sslHandle);
        mbedtls_ssl_config_free(&ptConnCtx->sslConfig);
        mbedtls_ctr_drbg_free(&ptConnCtx->ctr_drbg);
        mbedtls_entropy_free(&ptConnCtx->entropy);
        free(ptConnCtx);
    }

    class _DummyWebSocket : public easywsclient::WebSocket {
    public:
        void poll(int timeout) { }
        void send(const std::string& message) { }
        void sendPing() { }
        void close() { }
        void _dispatch(Callback& callable) { }
        readyStateValues getReadyState() const { return CLOSED; }
    };

    class _RealWebSocket : public easywsclient::WebSocket {
    public:
        struct wsheader_type {
            unsigned header_size;
            bool fin;
            bool mask;
            enum opcode_type {
                CONTINUATION = 0x0,
                TEXT_FRAME = 0x1,
                BINARY_FRAME = 0x2,
                CLOSE = 8,
                PING = 9,
                PONG = 0xa,
            } opcode;
            int N0;
            uint64_t N;
            uint8_t masking_key[4];
        };

        std::vector<uint8_t> rxbuf;
        std::vector<uint8_t> txbuf;
        std::vector<uint8_t> receivedData;

        ConnectionContext* ptConnCtx;
        readyStateValues readyState;
        bool useMask;

        _RealWebSocket(ConnectionContext* connContext, bool useMask)
            : ptConnCtx(connContext), readyState(OPEN), useMask(useMask) {}

        readyStateValues getReadyState() const {
            return readyState;
        }

        void poll(int timeout) {
            if (readyState == CLOSED) {
                if (timeout > 0) {
                    timeval tv = { timeout / 1000, (timeout % 1000) * 1000 };
                    select(0, NULL, NULL, NULL, &tv);
                }
                return;
            }
            if (timeout > 0) {
                fd_set rfds;
                fd_set wfds;
                timeval tv = { timeout / 1000, (timeout % 1000) * 1000 };
                FD_ZERO(&rfds);
                FD_ZERO(&wfds);
                FD_SET(ptConnCtx->sockfd, &rfds);
                if (!txbuf.empty()) { FD_SET(ptConnCtx->sockfd, &wfds); }
                select((int)ptConnCtx->sockfd + 1, &rfds, &wfds, NULL, &tv);
            }
            while (true) {
                int N = rxbuf.size();
                ssize_t ret;
                rxbuf.resize(N + 1500);
                ret = kRead(ptConnCtx, (char*)rxbuf.data() + N, 1500, 0);
                if (ret < 0 && (socketerrno == SOCKET_EWOULDBLOCK || socketerrno == SOCKET_EAGAIN_EINPROGRESS)) {
                    rxbuf.resize(N);
                    break;
                }
                else if (ret <= 0) {
                    rxbuf.resize(N);
                    klose(ptConnCtx);
                    readyState = CLOSED;
                    log_error(ret < 0 ? "Connection error!\n" : "Connection closed!\n");
                    break;
                }
                else {
                    rxbuf.resize(N + ret);
                }
            }
            while (!txbuf.empty()) {
                if (readyState == CLOSED) {
                    txbuf.clear();
                    break;
                }
                int ret = kWrite(ptConnCtx, (char*)txbuf.data(), txbuf.size(), 0);
                if (ret < 0 && (socketerrno == SOCKET_EWOULDBLOCK || socketerrno == SOCKET_EAGAIN_EINPROGRESS)) {
                    break;
                }
                else if (ret <= 0) {
                    klose(ptConnCtx);
                    readyState = CLOSED;
                    log_error(ret < 0 ? "Connection error!\n" : "Connection closed!\n");
                    break;
                }
                else {
                    txbuf.erase(txbuf.begin(), txbuf.begin() + ret);
                }
            }
            if (txbuf.empty() && readyState == CLOSING) {
                klose(ptConnCtx);
                readyState = CLOSED;
            }
        }

        virtual void _dispatch(WebSocket::Callback& callable) {
            while (true) {
                wsheader_type ws;
                if (rxbuf.size() < 2) { return; }
                const uint8_t* data = (uint8_t*)rxbuf.data();
                ws.fin = (data[0] & 0x80) == 0x80;
                ws.opcode = (wsheader_type::opcode_type)(data[0] & 0x0f);
                ws.mask = (data[1] & 0x80) == 0x80;
                ws.N0 = (data[1] & 0x7f);
                ws.header_size = 2 + (ws.N0 == 126 ? 2 : 0) + (ws.N0 == 127 ? 6 : 0) + (ws.mask ? 4 : 0);
                if (rxbuf.size() < ws.header_size) { return; }
                int i;
                if (ws.N0 < 126) {
                    ws.N = ws.N0;
                    i = 2;
                }
                else if (ws.N0 == 126) {
                    ws.N = 0;
                    ws.N |= ((uint64_t)data[2]) << 8;
                    ws.N |= ((uint64_t)data[3]) << 0;
                    i = 4;
                }
                else if (ws.N0 == 127) {
                    ws.N = 0;
                    ws.N |= ((uint64_t)data[2]) << 56;
                    ws.N |= ((uint64_t)data[3]) << 48;
                    ws.N |= ((uint64_t)data[4]) << 40;
                    ws.N |= ((uint64_t)data[5]) << 32;
                    ws.N |= ((uint64_t)data[6]) << 24;
                    ws.N |= ((uint64_t)data[7]) << 16;
                    ws.N |= ((uint64_t)data[8]) << 8;
                    ws.N |= ((uint64_t)data[9]) << 0;
                    i = 10;
                }
                if (ws.mask) {
                    ws.masking_key[0] = ((uint8_t)data[i + 0]) << 0;
                    ws.masking_key[1] = ((uint8_t)data[i + 1]) << 0;
                    ws.masking_key[2] = ((uint8_t)data[i + 2]) << 0;
                    ws.masking_key[3] = ((uint8_t)data[i + 3]) << 0;
                }
                else {
                    ws.masking_key[0] = 0;
                    ws.masking_key[1] = 0;
                    ws.masking_key[2] = 0;
                    ws.masking_key[3] = 0;
                }
                if (rxbuf.size() < ws.header_size + ws.N) { return; }
                if (ws.opcode == wsheader_type::TEXT_FRAME || ws.opcode == wsheader_type::CONTINUATION) {
                    if (ws.mask) { for (size_t i = 0; i != ws.N; ++i) { rxbuf[i + ws.header_size] ^= ws.masking_key[i & 0x3]; } }
                    receivedData.insert(receivedData.end(), rxbuf.begin() + ws.header_size, rxbuf.begin() + ws.header_size + (size_t)ws.N);
                    if (ws.fin) {
                        std::string data(receivedData.begin(), receivedData.end());
                        callable((const std::string)data);
                        receivedData.erase(receivedData.begin(), receivedData.end());
                        std::vector<uint8_t>().swap(receivedData);
                    }
                }
                else if (ws.opcode == wsheader_type::PING) {
                    if (ws.mask) { for (size_t i = 0; i != ws.N; ++i) { rxbuf[i + ws.header_size] ^= ws.masking_key[i & 0x3]; } }
                    std::string data(rxbuf.begin() + ws.header_size, rxbuf.begin() + ws.header_size + (size_t)ws.N);
                    sendData(wsheader_type::PONG, data);
                }
                else if (ws.opcode == wsheader_type::PONG) {
                    // No action required
                }
                else if (ws.opcode == wsheader_type::CLOSE) {
                    close();
                }
                else {
                    log_error("ERROR: Got unexpected WebSocket message. opcode=%d\n", ws.opcode);
                    close();
                }
                rxbuf.erase(rxbuf.begin(), rxbuf.begin() + ws.header_size + (size_t)ws.N);
            }
        }

        void sendPing() {
            sendData(wsheader_type::PING, std::string());
        }

        void send(const std::string& message) {
            sendData(wsheader_type::TEXT_FRAME, message);
        }

        void sendData(wsheader_type::opcode_type type, const std::string& message) {
            const uint8_t masking_key[4] = { 0x12, 0x34, 0x56, 0x78 };
            if (readyState == CLOSING || readyState == CLOSED) { return; }
            std::vector<uint8_t> header;
            uint64_t message_size = message.size();
            header.assign(2 + (message_size >= 126 ? 2 : 0) + (message_size >= 65536 ? 6 : 0) + (useMask ? 4 : 0), 0);
            header[0] = 0x80 | type;
            if (message_size < 126) {
                header[1] = (message_size & 0xff) | (useMask ? 0x80 : 0);
                if (useMask) {
                    header[2] = masking_key[0];
                    header[3] = masking_key[1];
                    header[4] = masking_key[2];
                    header[5] = masking_key[3];
                }
            }
            else if (message_size < 65536) {
                header[1] = 126 | (useMask ? 0x80 : 0);
                header[2] = (message_size >> 8) & 0xff;
                header[3] = (message_size >> 0) & 0xff;
                if (useMask) {
                    header[4] = masking_key[0];
                    header[5] = masking_key[1];
                    header[6] = masking_key[2];
                    header[7] = masking_key[3];
                }
            }
            else {
                header[1] = 127 | (useMask ? 0x80 : 0);
                header[2] = (message_size >> 56) & 0xff;
                header[3] = (message_size >> 48) & 0xff;
                header[4] = (message_size >> 40) & 0xff;
                header[5] = (message_size >> 32) & 0xff;
                header[6] = (message_size >> 24) & 0xff;
                header[7] = (message_size >> 16) & 0xff;
                header[8] = (message_size >> 8) & 0xff;
                header[9] = (message_size >> 0) & 0xff;
                if (useMask) {
                    header[10] = masking_key[0];
                    header[11] = masking_key[1];
                    header[12] = masking_key[2];
                    header[13] = masking_key[3];
                }
            }
            txbuf.insert(txbuf.end(), header.begin(), header.end());
            txbuf.insert(txbuf.end(), message.begin(), message.end());
            if (useMask) {
                for (size_t i = 0; i != message.size(); ++i) { *(txbuf.end() - message.size() + i) ^= masking_key[i & 0x3]; }
            }
        }

        void close() {
            if (readyState == CLOSING || readyState == CLOSED) { return; }
            readyState = CLOSING;
            uint8_t closeFrame[6] = { 0x88, 0x80, 0x00, 0x00, 0x00, 0x00 }; // last 4 bytes are a masking key
            std::vector<uint8_t> header(closeFrame, closeFrame + 6);
            txbuf.insert(txbuf.end(), header.begin(), header.end());
        }

    };

    easywsclient::WebSocket::pointer from_url(const std::string& url, bool useMask, const std::string& origin, const std::string& api_key) {
        char sc[128] = { 0 };
        char host[128] = { 0 };
        int port = 0;
        char path[128] = { 0 };
        bool is_ssl = false;
        if (url.size() >= 128) {
            log_error("ERROR: url size limit exceeded: %s\n", url.c_str());
            return NULL;
        }
        if (origin.size() >= 200) {
            log_error("ERROR: origin size limit exceeded: %s\n", origin.c_str());
            return NULL;
        }
        if (sscanf(url.c_str(), "w%[s]://%[^:/]:%d/%s", sc, host, &port, path) != 4 &&
            sscanf(url.c_str(), "w%[s]://%[^:/]/%s", sc, host, path) != 3 &&
            sscanf(url.c_str(), "w%[s]://%[^:/]:%d", sc, host, &port) != 3 &&
            sscanf(url.c_str(), "w%[s]://%[^:/]", sc, host) != 2) {
            log_error("ERROR: Could not parse WebSocket url: %s\n", url.c_str());
            return NULL;
        }
        is_ssl = sc[1] == 's';
        if (is_ssl && port == 0) port = 443;
        if (!is_ssl && port == 0) port = 80;

        log_error("easywsclient: connecting: ssl=%s, host=%s port=%d path=/%s\n",
            is_ssl ? "true" : "false", host, port, path);

        ConnectionContext* ptConnCtx = (ConnectionContext*)calloc(1, sizeof(ConnectionContext));

        ptConnCtx->sockfd = hostname_connect(host, port);
        if (ptConnCtx->sockfd == INVALID_SOCKET) {
            log_error("ERROR: Unable to connect to %s:%d\n", host, port);
            free(ptConnCtx);
            return NULL;
        }

        if (is_ssl) {
            ptConnCtx->is_ssl = true;
            mbedtls_ssl_init(&ptConnCtx->sslHandle);
            mbedtls_ssl_config_init(&ptConnCtx->sslConfig);
            mbedtls_ctr_drbg_init(&ptConnCtx->ctr_drbg);
            mbedtls_entropy_init(&ptConnCtx->entropy);

            const char* pers = "ssl_client";
            if (mbedtls_ctr_drbg_seed(&ptConnCtx->ctr_drbg, mbedtls_entropy_func, &ptConnCtx->entropy, (const unsigned char*)pers, strlen(pers)) != 0) {
                log_error("ERROR: mbedtls_ctr_drbg_seed() failed\n");
                klose(ptConnCtx);
                return NULL;
            }

            if (mbedtls_ssl_config_defaults(&ptConnCtx->sslConfig, MBEDTLS_SSL_IS_CLIENT, MBEDTLS_SSL_TRANSPORT_STREAM, MBEDTLS_SSL_PRESET_DEFAULT) != 0) {
                log_error("ERROR: mbedtls_ssl_config_defaults() failed\n");
                klose(ptConnCtx);
                return NULL;
            }

            mbedtls_ssl_conf_authmode(&ptConnCtx->sslConfig, MBEDTLS_SSL_VERIFY_OPTIONAL);
            mbedtls_ssl_conf_rng(&ptConnCtx->sslConfig, mbedtls_ctr_drbg_random, &ptConnCtx->ctr_drbg);

            if (mbedtls_ssl_setup(&ptConnCtx->sslHandle, &ptConnCtx->sslConfig) != 0) {
                log_error("ERROR: mbedtls_ssl_setup() failed\n");
                klose(ptConnCtx);
                return NULL;
            }

            if (mbedtls_ssl_set_hostname(&ptConnCtx->sslHandle, host) != 0) {
                log_error("ERROR: mbedtls_ssl_set_hostname() failed\n");
                klose(ptConnCtx);
                return NULL;
            }

            mbedtls_ssl_set_bio(&ptConnCtx->sslHandle, &ptConnCtx->sockfd, mbedtls_net_send, mbedtls_net_recv, NULL);

            if (mbedtls_ssl_handshake(&ptConnCtx->sslHandle) != 0) {
                log_error("ERROR: mbedtls_ssl_handshake() failed\n");
                klose(ptConnCtx);
                return NULL;
            }
        }

        {
            char line[256];
            int status;
            int i;
            snprintf(line, 256, "GET /%s HTTP/1.1\r\n", path); kWrite(ptConnCtx, line, strlen(line), 0);
            if (port == 80) {
                snprintf(line, 256, "Host: %s\r\n", host); kWrite(ptConnCtx, line, strlen(line), 0);
            }
            else {
                snprintf(line, 256, "Host: %s:%d\r\n", host, port); kWrite(ptConnCtx, line, strlen(line), 0);
            }
            snprintf(line, 256, "Upgrade: websocket\r\n"); kWrite(ptConnCtx, line, strlen(line), 0);
            if (!api_key.empty())
                snprintf(line, 256, "X-Api-Key: %s\r\n", api_key.c_str()); kWrite(ptConnCtx, line, strlen(line), 0);
            snprintf(line, 256, "Connection: keep-alive, Upgrade\r\n"); kWrite(ptConnCtx, line, strlen(line), 0);
            if (!origin.empty()) {
                snprintf(line, 256, "Origin: %s\r\n", origin.c_str()); kWrite(ptConnCtx, line, strlen(line), 0);
            }
            snprintf(line, 256, "Sec-WebSocket-Key: hLuO7MKwvHBxsv/ureQI9w==\r\n"); kWrite(ptConnCtx, line, strlen(line), 0);
            snprintf(line, 256, "Sec-WebSocket-Version: 13\r\n"); kWrite(ptConnCtx, line, strlen(line), 0);
            snprintf(line, 256, "\r\n"); kWrite(ptConnCtx, line, strlen(line), 0);
            for (i = 0; i < 2 || (i < 255 && line[i - 2] != '\r' && line[i - 1] != '\n'); ++i) {
                if (kRead(ptConnCtx, line + i, 1, 0) == 0) { return NULL; }
            }
            line[i] = 0;
            if (i == 255) {
                log_error("ERROR: Got invalid status line connecting to: %s\n", url.c_str());
                return NULL;
            }
            if (sscanf(line, "HTTP/1.1 %d", &status) != 1 || status != 101) {
                log_error("ERROR: Got bad status connecting to %s: %s", url.c_str(), line);
                return NULL;
            }
            while (true) {
                for (i = 0; i < 2 || (i < 255 && line[i - 2] != '\r' && line[i - 1] != '\n'); ++i) {
                    if (kRead(ptConnCtx, line + i, 1, 0) == 0) { return NULL; }
                }
                if (line[0] == '\r' && line[1] == '\n') { break; }
            }
        }

        int flag = 1;
        setsockopt(ptConnCtx->sockfd, IPPROTO_TCP, TCP_NODELAY, (char*)&flag, sizeof(flag));

#ifdef _WIN32
        u_long on = 1;
        ioctlsocket(ptConnCtx->sockfd, FIONBIO, &on);
#else
        fcntl(ptConnCtx->sockfd, F_SETFL, O_NONBLOCK);
#endif

        log_error("Connected to: %s\n", url.c_str());
        return easywsclient::WebSocket::pointer(new _RealWebSocket(ptConnCtx, useMask));
    }

} // end of module-only namespace

namespace easywsclient {

    WebSocket::pointer WebSocket::create_dummy() {
        static pointer dummy = pointer(new _DummyWebSocket);
        return dummy;
    }

    WebSocket::pointer WebSocket::from_url(const std::string& url, const std::string& origin, const std::string& api_key) {
        return ::from_url(url, true, origin, api_key);
    }

    WebSocket::pointer WebSocket::from_url_no_mask(const std::string& url, const std::string& origin) {
        return ::from_url(url, false, origin, {});
    }

} // namespace easywsclient
