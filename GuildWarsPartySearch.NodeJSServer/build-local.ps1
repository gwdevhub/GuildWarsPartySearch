echo "This script builds a docker container called partysearch with a configuration based on Config.json from the local files"

docker build --no-cache -t partysearch.nodejs -f Dockerfile-Local .
docker stop partysearch
docker rm partysearch
docker create --name partysearch -p 8080:80 partysearch.nodejs:latest
docker start partysearch