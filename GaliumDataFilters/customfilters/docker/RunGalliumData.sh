#!/bin/sh

cd /galliumdata

exec /GraalVM/bin/java \
  -classpath "/galliumdata/jars/*" \
  -Xms4g -Xmx4g -XX:+UseZGC \
  --add-exports org.graalvm.truffle/com.oracle.truffle.api.debug=com.galliumdata.engine \
  --add-exports org.graalvm.truffle/com.oracle.truffle.api.source=com.galliumdata.engine \
  --add-exports org.graalvm.truffle/com.oracle.truffle.api.instrumentation=com.galliumdata.engine \
  --add-exports org.graalvm.truffle/com.oracle.truffle.polyglot=com.galliumdata.engine \
  --add-opens org.graalvm.truffle/com.oracle.truffle.api.debug=com.galliumdata.engine \
  --add-opens org.graalvm.truffle/com.oracle.truffle.api.source=com.galliumdata.engine \
  --add-opens org.graalvm.truffle/com.oracle.truffle.api.instrumentation=com.galliumdata.engine \
  --add-opens org.graalvm.truffle/com.oracle.truffle.polyglot=com.galliumdata.engine \
  --add-exports org.graalvm.truffle/com.oracle.truffle.api.debug=ALL-UNNAMED \
  --add-exports org.graalvm.truffle/com.oracle.truffle.api.source=ALL-UNNAMED \
  --add-exports org.graalvm.truffle/com.oracle.truffle.api.instrumentation=ALL-UNNAMED \
  --add-opens java.desktop/sun.font=ALL-UNNAMED \
  --add-opens java.base/java.lang.invoke=ALL-UNNAMED  \
  --add-opens java.base/java.lang.invoke=retrofit2  \
  -Djdk.tls.client.protocols="TLSv1,TLSv1.1,TLSv1.2" \
  -Dlog4j.configurationFile=/galliumdata/settings/log4j2.properties \
  -Dlog4j.configuration=file:///galliumdata/settings/log4j.properties \
  -Xdebug -Xrunjdwp:transport=dt_socket,address=*:8000,server=y,suspend=y \
  --module com.galliumdata.engine/com.galliumdata.server.Main \
  --settings-location=/galliumdata/settings/default.properties

# Add this to the command line after verbose:gc for remote debugging
#  -Xdebug -Xrunjdwp:transport=dt_socket,address=*:8000,server=y,suspend=y \

# Add this to get a heap dump on out of memory
#  -XX:+HeapDumpOnOutOfMemoryError -XX:HeapDumpPath=/debug/heapdump.bin \

#  -Xlog:gc \
#  -XX:+HeapDumpOnOutOfMemoryError \
#  -XX:HeapDumpPath=/debug/heapdump.bin \
#
#  | tee /debug/GalliumData-`date +"%Y%m%d-%H%M%S"`.out

#   -classpath "/galliumdata/jars/*" \
