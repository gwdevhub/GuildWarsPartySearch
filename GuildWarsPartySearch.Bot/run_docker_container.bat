docker build -t partysearchbot_alpine .
docker run -d --name partysearchbot -v "%cd%":/app partysearchbot_alpine