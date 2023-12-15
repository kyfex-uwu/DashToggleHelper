local mods = require("mods")

local lib={}

lib.maxDashes = 3
if mods.hasLoadedMod("MoreDasheline") then
    lib.maxDashes = 6
end

local colors = {
    {68, 183, 255},
    {172, 50, 50},
    {255, 109, 239},
    {0, 128, 0},
    {255, 255, 0},
    {255, 0, 255}
}
for i=1, #colors, 1 do
    for j=1, 3, 1 do
        colors[i][j]=colors[i][j]/255
    end
    colors[i][4]=1
end
lib.getColor = function(color)
    return colors[math.fmod(color or 0, lib.maxDashes)+1]
end

lib.getDashProp = function()
    local toReturn={}
    for i=0,lib.maxDashes,1 do
        toReturn[i]=i..""
    end
    return toReturn
end

return lib