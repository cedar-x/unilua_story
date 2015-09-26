

_G.StorySystem = CSystem:Create("StorySystem");
StorySystem.objScene = nil; 	--story scene
StorySystem.cfgStory = nil;	--current story config
StorySystem.tbTimeActors = {};	--有TimeEvent事件的Actor


function StorySystem:Create()
	
end

function StorySystem:OnEnterScene(bEditorMode)
	if bEditorMode then
		-- Game:InitMainCamera();
		-- self:Play(Game:GetStoryScriptID());
		self:Play(1001);
	end
end

function StorySystem:Update()
	local dwNow = GetTime();
	for i, objActor in pairs(self.tbTimeActors) do
		objActor:Update(dwNow);
	end
end
--基本函数
function StorySystem:GetStory()
	return self.cfgStory;
end
function StorySystem:Play(dwStoryID)
	print("StorySystem:Play:", dwStoryID);
	self.cfgStory = self:LoadScript(dwStoryID);
	if self.cfgStory == nil then
		return
	end
	-- self.cfgStory:SetStoryID(dwStoryID);

	-- self:PushState();
	-- UIStory:Create();

	-- local objMainActor = self:CPMainPlayer();

	local b,info = pcall(self.cfgStory.OnStoryStart,self.cfgStory, "123");
	if b == false then
		warn("StorySystem Play Failed "..info);
		self:Stop();
	end
end

function StorySystem:Stop()
	print("StorySystem:Stop:......")
	self.tbTimeActors = {};
	if self.cfgStory == nil then
		return
	end

	-- self:PopState();
	self.cfgStory = nil;

	-- _System:GarbageCollect();
end

--private
function StorySystem:LoadScript(dwStoryID)
	local szScriptName = string.format("StoryScript_%d",dwStoryID);
	if _G[szScriptName] == nil then
		local szFile = string.format("ScriptConfig/StoryConfig/%s.lua",szScriptName);
		dofile(szFile)
	end
	return _G[szScriptName];
end
--[[
function StorySystem:PushState()
	UIManager:PushState("Story");
	-- UIManager:MakeDefault();
	Input:PushState();
	Game:GetScene():PushRenderSetting();
	UITipManager:Hide();
	local objMainCamera = Game:GetMainCamera();
	objMainCamera:SavePoint();
	objMainCamera:LookTarget();
	Game:AddExtraCamera("MainCamera-juqing")
	self.cfgStory.objMainCamera = Game:GetExtraCamera();
	self.cfgStory.objMainCamera.type = 1;
	self:DoEvent("OnStartStoryPlay");
	if _G["CCommandMgr"] then
		self.actionFn = CCommandMgr:PauseMainActCommond()
		Game:StopEffectType(Player, "JN/");
		local objMagicWp = Player:GetMagicWeapon();
		if objMagicWp ~= nil then
			Game:StopEffectType(objMagicWp, "Fabao/");
		end
	end
	Input:AddKeyboardListener(function(dwKeyCode, dwKeyDown)
		if dwKeyCode == KeyCode.Escape then
			self:Stop();
			return
		end
	end);
	if self.cfgStory:bNewScene() then
		Game:GetScene():HideAll();
	end
	self.objScene = Game:CreateScene();
end
function StorySystem:PopState()
	--还原场景
	if self.cfgStory:bNewScene() then
		Game:GetScene():ShowAll();
	end
	Game:DestroyScene(self.objScene);
	self.objScene = nil;
	--
	Game:GetScene():PopRenderSetting();
	UIManager:PopState();
	Input:PopState();
	UIStory:ShowInfo(" ", " ");
	Game:RemoveExtraCamera("MainCamera-juqing")
	if self.actionFn then 
		self.actionFn();
		self.actionFn = nil;
	end
	local objCamera = Game:GetMainCamera();

	if objCamera then 
		objCamera:BackPoint();
	end
	self:DoEvent("OnEndStoryPlay", self.taskID);
end

--创建剧情动画中的角色
--角色名称
--角色的骨骼文件
--x,y,z 角色初始化坐标位置 
--fDir, 角色初始化朝向
function StorySystem:CActor(name,szSkeleton,x,y,z,fDir)

	local objActor = self:GetEntity(name);
	if objActor then
		self:DActor(objActor);
	end

	objActor = self.objScene:CreateActor(name,szSkeleton);
	if objActor == nil then
		return
	end
	objActor.type = ActorType.Monster;
	StoryActor:Init(objActor);

	if x and y and z then
		objActor:SetLocalPosition(x,y,z);
	end

	if fDir then
		objActor:SetLocalRotate(0,fDir,0);
	end

	return objActor;
end

--删除角色
--name 可以是角色的对象，也可以是创建时的名称
function StorySystem:DActor(name,fDelay)
	if name == nil then
		return
	end

	local objActor = self:GetEntity(name);
	if objActor == nil then
		return
	end

	self.tbTimeActors[objActor:GetID()] = nil;

	return self.objScene:RemoveActor(objActor,fDelay);
end

function StorySystem:GetEntity(name)
	local dwID = 0;
	if type(name) == "string" then
		dwID = Game:GetStringHash(name);
	else
		dwID = name;
	end
	return self.objScene:GetEntity(dwID);
end

function StorySystem:CPlayer(name,cfgPlayer,x,y,z,fDir)
	local objActor = self:CActor(name,cfgPlayer.skeleton,x,y,z,fDir);
	if objActor == nil then
		return
	end

	return objActor;
end

--创建剧情动画中的虚拟对象，用于定位虚拟空间
--角色名称
--szSkeleton为了保持与CActor参数统一，无作用
--x,y,z 虚拟对象初始化坐标位置 
--fDir, 虚拟对象初始化朝向
function StorySystem:CEmpty(name,szSkeleton,x,y,z,fDir)
	local objActor = self:GetEntity(name);
	if objActor then
		self:DActor(objActor);
	end

	objActor = self.objScene:CreateEmptyObject(name);
	if objActor == nil then
		return
	end
	objActor.type = ActorType.Entity;
	StoryActor:Init(objActor);
	if x and y and z then
		objActor:SetLocalPosition(x,y,z);
	end
	if fDir then
		objActor:SetLocalRotate(0,fDir,0);
	end
	return objActor;
end;

---------------------------------------------------

function StorySystem:GetScene()
	return self.objScene;
end



--private
function StorySystem:AddTimeActor(objActor)
	self.tbTimeActors[objActor:GetID()] = objActor;
end

--private
function StorySystem:RemoveTimeActor(objActor)
	if not objActor then return end;
	objActor:RemoveEvent();
	self.tbTimeActors[objActor:GetID()] = nil;
end

--]]