# see: https://www.postgresql.org/docs/14/install-procedure.html

cpuArchitecture=$(uname -p)
postgresSource="https://github.com/postgres/postgres/archive/refs/tags"
scriptDir="$(realpath $(dirname ${0}))"
buildDir="${scriptDir}/postgres-build"
outputDir="${buildDir}/linux"
postgresVersion="${1:-"14_1"}"
postgresArchive="REL_${postgresVersion}.zip"

rm -rf "${buildDir}"

mkdir "${buildDir}"
cd "${buildDir}"

wget "${postgresSource}/${postgresArchive}"
unzip "${postgresArchive}"

cd "postgres-REL_${postgresVersion}"
chmod +x "configure"

./configure \
  --prefix="${outputDir}" \
  --with-uuid="ossp"

make
make check
make install-strip

mkdir -p "${scriptDir}/postgres/linux"

mv "${outputDir}" "${scriptDir}/postgres/linux/${cpuArchitecture}"
