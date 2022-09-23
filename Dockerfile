## use the dotnet sdk image to build/compile the project code
#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
#WORKDIR /app
## install nodejs since this is involves the angular framework
#RUN curl -sL https://deb.nodesource.com/setup_18.x | bash -
#ARG DEBIAN_FRONTEND=noninteractive
#RUN apt update && apt install -y nodejs wget
## Copy csproj and restore as distinct layers
#COPY *.csproj ./
#RUN dotnet restore
## Copy everything else and build
#COPY . .
#RUN dotnet restore
#RUN dotnet publish -c Debug -o out -r linux-x64
# now use the runtime image to run the code
#FROM mcr.microsoft.com/dotnet/aspnet:6.0 as run-env
#FROM selenium/standalone-chrome:latest
FROM mcr.microsoft.com/dotnet/sdk:6.0 as run-env
# we need nodejs here as well
WORKDIR /app
RUN apt-get update && apt-get install -y nodejs wget unzip gnupg curl
# add latest stable chrome installation
RUN curl -s https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - && echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' | tee /etc/apt/sources.list.d/google-chrome.list && apt-get update && apt-get install -y google-chrome-stable
RUN google-chrome --version | grep -oE "[0-9]{1,10}.[0-9]{1,10}.[0-9]{1,10}" > /tmp/chromebrowser-main-version.txt
RUN wget --no-verbose -O /tmp/latest_chromedriver_version.txt https://chromedriver.storage.googleapis.com/LATEST_RELEASE_$(cat /tmp/chromebrowser-main-version.txt)
# Install chromedriver for Selenium
RUN wget --no-verbose -O /tmp/chromedriver_linux64.zip https://chromedriver.storage.googleapis.com/$(cat /tmp/latest_chromedriver_version.txt)/chromedriver_linux64.zip && rm -rf /opt/selenium/chromedriver && unzip /tmp/chromedriver_linux64.zip -d /opt/selenium && rm /tmp/chromedriver_linux64.zip && mv /opt/selenium/chromedriver /opt/selenium/chromedriver-$(cat /tmp/latest_chromedriver_version.txt) && chmod 755 /opt/selenium/chromedriver-$(cat /tmp/latest_chromedriver_version.txt) && ln -fs /opt/selenium/chromedriver-$(cat /tmp/latest_chromedriver_version.txt) /usr/bin/chromedriver
COPY appsettings* .
COPY ./bin/Debug/net6.0/* .
ENTRYPOINT ["dotnet", "trinibytes.dll"]
#ENTRYPOINT ["./trinibytes"]
#ENTRYPOINT ["tail","-f","/dev/null"]