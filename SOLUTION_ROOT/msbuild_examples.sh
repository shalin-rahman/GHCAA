# run a Clean target via msbuild (do not mix with restore)
dotnet msbuild -t:Clean
# then restore
dotnet restore	