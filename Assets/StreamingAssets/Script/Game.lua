

_G.Game = {}

function Game:Init(gameApp)

	gameApp.objState = nil;

	for i, v in pairs(self) do
		if type(v) == "function" then
			gameApp[i] = v;
		end
	end
	Game = gameApp;
end

function Game:GetCurrentState()
	return self.objState;
end

function Game:EnterState(sysState)
	if self.objState then
		self.objState:OnLeaveState();
	end

	self.objState = sysState;
	if self.objState then
		self.objState:OnEnterState();
	end
end

function Game:Update()
	if self.objState ~= nil then
		self.objState:Update();
	end
end

function Game:OnServerClose()
	if self.objState ~= nil then
		self.objState:OnServerClose();
	end
end

--用于缓存要进入场景的ID， 如果是世界场景，那么场景ID和区域编号一样
--如果是副本场景，那么场景ID应该是上次选择的ID，缓存在这里
function Game:SetEnterSceneID(dwSceneID)
	self.dwSceneID = dwSceneID;
end

function Game:GetEnterSceneID()
	return self.dwSceneID;
end
-----------------------------------------------
