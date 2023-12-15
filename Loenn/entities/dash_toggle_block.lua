local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local dashToggleBlock = {
    name="DashToggleHelper/DashToggleBlock",
    minimumSize = {16,16},
    fieldInformation={
        dashes = {
            options = {"0","1","2"}
        }
    },
    placements={
        {
            name="zeroDashes",
            data = {
                dashes="0",
                width = 16,
                height = 16
            }
        },
        {
            name="oneDash",
            data = {
                dashes="1",
                width = 16,
                height = 16
            }
        },
        {
            name="twoDashes",
            data = {
                dashes="2",
                width = 16,
                height = 16
            }
        }
    }
}

local texture = "objects/DashToggleHelper/dashtoggleblock/Active"
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

local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.index == target.index and entity.dashes == target.dashes
    end
end

local function getTileSprite(entity, x, y, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0

        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8

        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16

        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24

        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0

        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16

        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8

        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8

        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0

        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0

        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16

        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(texture, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite:setColor(colors[entity.dashes + 1] or colors[1])

        sprite.depth = -10

        return sprite
    end
end

function dashToggleBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    for x = 1, math.ceil((entity.width or 32) / 8) do
        for y = 1, math.ceil((entity.height or 32) / 8) do
            local sprite = getTileSprite(entity, x, y, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return dashToggleBlock
