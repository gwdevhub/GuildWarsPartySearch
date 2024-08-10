FROM alpine:3.20

RUN apk update && apk add \
  bash \
  cmake \
  ninja \
  ninja-build \
  alpine-sdk \
  python3 py3-pip py3-tqdm py3-pefile

WORKDIR /app

CMD ["sh", "-c", "while :; do sleep 2073600; done"]