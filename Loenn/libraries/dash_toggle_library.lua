local mods = require("mods")

local lib={}

lib.maxDashes = 3
lib.infDashes=false
if mods.hasLoadedMod("MoreDasheline") then
    lib.maxDashes = 4
    lib.infDashes=true
end

local colors = {
    {68, 183, 255},
    {172, 50, 50},
    {255, 109, 239},
    {0,128,0},
    {255,255,0},
    {255,0,255},
}
for i=1, #colors, 1 do
    for j=1, 3, 1 do
        colors[i][j]=colors[i][j]/255
    end
    colors[i][4]=1
end
lib.getColor = function(color)
    local toReturn = {1,1,1,1}
    pcall(function() toReturn = colors[tonumber(color)+1] end)--bro
    return toReturn
end

lib.associatedMods = function(entity)
    if tonumber(entity.dashes) > 2 then return {"DashToggleHelper","MoreDasheline"} end
    return {"DashToggleHelper"}
end

return lib