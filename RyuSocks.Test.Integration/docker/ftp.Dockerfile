FROM delfer/alpine-ftp-server

RUN mkdir -p /test/upload \
    && chown 1000:1000 -R /test

COPY vsftpd.conf /etc/vsftpd/vsftpd.conf
COPY --chown=1000:1000 assets /test/assets
