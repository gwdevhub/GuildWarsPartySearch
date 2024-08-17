FROM alpine:3.20

WORKDIR /app

RUN apk update && apk add bash nodejs npm

EXPOSE 80

CMD ["sh", "-c", "while :; do sleep 2073600; done"]