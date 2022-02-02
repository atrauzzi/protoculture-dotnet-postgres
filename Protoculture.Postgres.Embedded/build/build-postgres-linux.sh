# see: https://www.postgresql.org/docs/14/install-procedure.html

cpuArchitecture=$(uname -p)
postgresSource="https://github.com/postgres/postgres/archive/refs/tags"
scriptDir="$(realpath $(dirname ${0}))"
buildDir="${scriptDir}/postgres-build"
postgresBuildsDir="${scriptDir}/postgres"
outputDir="${postgresBuildsDir}/linux/${cpuArchitecture}"
postgresVersion="${1:-"14_1"}"
postgresArchive="REL_${postgresVersion}.zip"

rm -rf "${buildDir}"
rm -rf "${postgresBuildsDir}"

mkdir -p "${outputDir}"

mkdir "${buildDir}"
cd "${buildDir}"

wget "${postgresSource}/${postgresArchive}"
unzip "${postgresArchive}"

cd "postgres-REL_${postgresVersion}"
chmod +x "configure"

./configure \
  --disable-rpath \
  --prefix="${outputDir}" \
  --with-uuid="ossp"

export LD_RUN_PATH='$ORIGIN/../lib'

make
make check
make install-strip

rm -rf "${outputDir}/include"

cd "${postgresBuildsDir}"
zip -r "linux-${cpuArchitecture}.zip" .
