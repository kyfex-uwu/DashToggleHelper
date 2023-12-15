local drawableSprite = require("structs.drawable_sprite")
local enums = require("consts.celeste_enums")
local utils = require("utils")

local spinner = {
    name="DashToggleHelper/DashToggleStaticSpinner",
    fieldInformation = {
        dashes = {
            options = {"0","1","2"}
        }
    },
    placements = {
        {
            name = "zeroDashes",
            data = {
                dashes="0",
                attachToSolid = false
            }
        },
        {
            name = "oneDash",
            data = {
                dashes="1",
                attachToSolid = false
            }
        },
        {
            name = "twoDashes",
            data = {
                dashes="2",
                attachToSolid = false
            }
        },
    }
}
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
        colors[i][j]=colors[i][j]
    end
    colors[i][4]=1
end

local function getSpinnerTexture(entity, foreground)
    local prefix = foreground and "fg" or "bg"

    return "objects/DashToggleHelper/dashtogglestaticspinner/"..prefix.."00"
end

local function getSpinnerSprite(entity, foreground)
    texture = getSpinnerTexture(entity, foreground)
    local toReturn = drawableSprite.fromTexture(texture, {x=entity.x, y=entity.y})
    toReturn:setColor(colors[entity.dashes + 1] or colors[1])
    return toReturn
end

local function getConnectionSprites(room, entity)
    -- TODO - This can create some overlaps, can be improved later

    local sprites = {}

    for _, target in ipairs(room.entities) do
        if target == entity then
            break
        end

        if entity._name == target._name and entity.attachToSolid == target.attachToSolid and entity.dashes == target.dashes then
            if utils.distanceSquared(entity.x, entity.y, target.x, target.y) < (24 * 24) then
                local connectorData = {
                    x = math.floor((entity.x + target.x) / 2),
                    y = math.floor((entity.y + target.y) / 2),
                    dashes = entity.dashes
                }
                local sprite = getSpinnerSprite(connectorData, false)

                sprite.depth = -8499

                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function spinner.sprite(room, entity)
    local sprites = getConnectionSprites(room, entity)
    local mainSprite = getSpinnerSprite(entity, true)

    table.insert(sprites, mainSprite)

    return sprites
end

function spinner.depth(room, entity)
    return -8500
end

function spinner.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return spinner