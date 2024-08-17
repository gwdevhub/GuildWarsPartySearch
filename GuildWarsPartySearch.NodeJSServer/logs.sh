#!/bin/bash

BOT_NAME=${PWD##*/}
docker logs --tail 1000 "$BOT_NAME";