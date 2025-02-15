dotnet run --project ./CakeBuild/CakeBuild.csproj -- "$@"
rm -rf "$VINTAGE_STORY/Mods/rpgdifficulty"
cp -r ./Releases/rpgdifficulty "$VINTAGE_STORY/Mods/rpgdifficulty"