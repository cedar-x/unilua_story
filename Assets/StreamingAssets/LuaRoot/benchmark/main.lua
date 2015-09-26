local function awake()
end

local function start()
    require "LuaRoot.benchmark.sci_mark"
end

local function update()
end

local function late_update()
end

local function fixed_update()
end

return {
    awake           = awake,
    start           = start,
    update          = update,
    late_update     = late_update,
    fixed_update    = fixed_update,
}

