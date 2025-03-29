local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local mods = require("mods")
local DTLib = mods.requireFromPlugin("libraries.dash_toggle_library")

local spinner = {
    name="DashToggleHelper/DashToggleStaticSpinner",
    fieldInformation = {
        dashes = {
            fieldType="integer",
            minimumValue=0,
            maximumValue=DTLib.infDashes and math.huge or 2,
        },
        prefix={
            default="objects/DashToggleHelper/dashtoggleblock/"
        },
    },
    placements = {},
    associatedMods=DTLib.associatedMods,
}
for i=0,2,1 do
    table.insert(spinner.placements, {
        name = i.."dash",
        data = {
            dashes=i.."",
            attachToSolid = false,
            prefix="objects/DashToggleHelper/dashtogglestaticspinner/",
        }
    })
end
table.insert(spinner.placements, {
    name = "Ndash",
    data = {
        dashes=3,
        attachToSolid = false,
        prefix="objects/DashToggleHelper/dashtogglestaticspinner/",
    }
})

local function getSpinnerTexture(entity, foreground)
    return entity.prefix..(foreground and "fg" or "bg").."00"
end

local function getSpinnerSprite(entity, foreground)
    texture = getSpinnerTexture(entity, foreground)
    local toReturn = drawableSprite.fromTexture(texture, {x=entity.x, y=entity.y})
    toReturn:setColor(DTLib.getColor(entity.dashes))
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
                    dashes = entity.dashes,
                    prefix = entity.prefix,
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