FROM galliumdata/gallium-data-engine:1.9.0-2157
LABEL com.galliumdata.docker.version="Gallium Data 1.9.0-2157 with Java filter"

COPY --chown=galliumdata:galliumdata customfilters-1.0.jar /galliumdata/jars/
COPY --chown=galliumdata:galliumdata jsqlparser-4.6.jar /galliumdata/jars/
ADD repo.tar /galliumdata/metarepo/filter_types/
USER root
RUN chown -R galliumdata:galliumdata /galliumdata/metarepo
