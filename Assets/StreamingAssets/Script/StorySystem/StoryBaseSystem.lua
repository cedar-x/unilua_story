

--摄像机缓动
local StorySmooth = {
		time = 15;
		easetype = EaseType.linear;
		LoopType = LoopType.none;
		oncomplete  = nil;
}

-------------------------------------------------------------
--剧情对话流程基类-----------------------------------------------------------
_G.StoryBaseSystem = {}

function StoryBaseSystem:Create()
	local obj = {}	
	setmetatable(obj,{__index=self});
	obj.objMainCamera = nil;					--主摄像机
	obj.objMainActor = nil;						--主角对象
	obj.dwStoryID = 0;							--剧情ID
	obj.dwStoryType = 0;						--剧情类型--标识主线、副本
	obj.dwStage = 1;							--私有，子类不要操作此变量--当前大阶段
	obj.dwMiniStage = 0; 						--私有，子类不要操作此变量--当前小阶段--所有阶段用此做控制
	obj.dwNextOption = 0;						--私有，子类不要操作此变量--下次选项阶段数--即小阶段数
	obj.storyConfig = {};						--剧情整体配置
	obj.isEnding = false; 						--私有，子类不要操作此变量--标识本阶段是否结束
	obj.isChange = true;						--私有，子类不要操作此变量--标识是否改变阶段数		
	obj.objPathObject = nil;						--Camera Path载体				

	--以下字段芈月废弃
	obj.bNewSceneStage = false;
	obj.nextTarget= nil; 						--下一次目标设定：暂废弃
	obj.dwNextTalkTime = false;					--本次小阶段到下次大阶段时间(同一个人物两次说话间隔)
	obj.dwNextTargetTime = false;				--本次大阶段到下次大阶段时间(切换当前人物间隔时间)
	obj.szStageName = ""; 						--章节名称
	obj.tabContent = {}; 						--本阶段对话内容
	obj.storyCameraConfig = {};
	obj.storyParamConfig = {};
	obj.storyActorConfig = {};

	return obj; 
end
--剧情逻辑流程
function StoryBaseSystem:OnNextStage()
	local dwVersion = self:GetVersion();
	if not dwVersion then
		self:OnHstjNextStage();
	else
		self:OnMyNextStage();
	end
end
function StoryBaseSystem:OnNowStageEnd(bNext)
	self.isEnding = true
	if bNext == false then return end;
	if bNext == 0 then 
		self:OnNextStage();
		return 
	end
	local dwTime = nil;
	local dwVersion = self:GetVersion();
	if not dwVersion then
		dwTime = self.storyParamConfig.dwNextTalkTime or self.dwNextTalkTime;
		if self.dwMiniStage == #(self.tabContent[self.dwStage] or {}) then 
			dwTime = self.storyParamConfig.dwNextTargetTime or self.dwNextTargetTime;
		end
	else
		dwTime = self.storyConfig.tabBasic.dwNextTalkTime;
		local tabNextContent = self.storyConfig.tabContent[self.dwMiniStage+1];
		if tabNextContent and self.dwStage ~= tabNextContent.dwStage then 
			dwTime = self.storyConfig.tabBasic.dwNextTargetTime;
		end
	end
	
	if type(dwTime)=="boolean" then return end;
	if not dwTime or dwTime == 0 then 
		self:OnNextStage();
		return;
	end
	self.objMainActor:TimeEvent(dwTime,function(this)
		self:OnNextStage();
	end);
end
--私有函数，标识是否改变阶段数
function StoryBaseSystem:changeState(bChange)
	self.isChange = bChange;
end
function StoryBaseSystem:GetChangeState()
	return self.isChange;
end
--剧情基础设置-----------------------------------------------------------
function StoryBaseSystem:SetStoryID(dwStoryID)
	self.dwStoryID = dwStoryID;
end
function  StoryBaseSystem:GetStoryID()
	return self.dwStoryID;
end
function StoryBaseSystem:GetVersion()
	local tabBasic = self.storyConfig.tabBasic;
	if not tabBasic then return end;
	if tabBasic.dwVersion == 1 then return end;
	return tabBasic.dwVersion;
end;
function StoryBaseSystem:GetType()
	local tabBasic = self.storyConfig.tabBasic;
	if not tabBasic then return end;
	return tabBasic.dwType;
end
function StoryBaseSystem:bNewScene()
	local tabBasic = self.storyConfig.tabBasic;
	--为了兼容以前self.bNewSceneStage,但是剧情应该都是新建一个scenestate(强制返回true)，否则剧情结束无法消除剧情模型
	if not tabBasic then return true; end;  
	return tabBasic.bNewSceneState;
end
function StoryBaseSystem:bNewLight()  
	local tabBasic = self.storyConfig.tabBasic;
	if not tabBasic then return end;  
	return tabBasic.bNewLightState;
end
function StoryBaseSystem:GetLightInfo()
	if not self:bNewLight() then return end;
	return self.storyConfig.tabLight;
end
function StoryBaseSystem:GetEnvInfo()
	return self.storyConfig.tabEnv;
end
--剧情入口
function StoryBaseSystem:OnStoryStart(objMainActor)--脚本名称改成文件名
	--不用动
	self.objMainActor = nil;
	--剧情初始设定
	self:OnInitStoryStart()
	--2.变量设定
	local szName = self.storyConfig.tabActor[1].name;
	self.objMainActor = self[szName];--调用虚拟主角，实际用objMainActor
    ------------------------------
    --结束第0阶段，即初始阶段
    self:OnNowStageEnd(0);
end
--剧情出口
function StoryBaseSystem:OnStopStory()
	if self.dwNextOption == 0 then 
		--还原灯光
		if self.objMainLight then 
			ResManager:DestroyObject(self.objMainLight)
			self.objMainLight = nil;
		end
		--结束剧情
		DStorySystem:Stop();
		return;
	end
	self.dwMiniStage = self.dwNextOption - 1;
	local tabContent = self.storyConfig.tabContent[self.dwMiniStage];
	self.dwStage = tabContent.dwStage;
	self.dwNextOption = self.storyConfig.tabContent[self.dwNextOption].dwNextOpt;
	self:OnNextStage();
end;
--初始设定
function StoryBaseSystem:OnInitStoryStart()
	local tabBasic = self.storyConfig.tabBasic;
	local tabActor = self.storyConfig.tabActor;
	if not tabBasic then error("can't find basic config storyID:", self.dwStoryID); return end;
	if not tabActor then error("is't there any Actor:", self.dwStoryID); return end;
	--1、初始标题
	UIStory:ShowTitle(tabBasic.szStageName);
	--2、初始环境光、灯光
	if self:bNewLight() then
		Game:GetScene():SetMainLightActive(false);
		self.objMainLight = Game:AddMainLight("MainLight_juqing");
		local tabLightInfo = self:GetLightInfo();
		local tabEnv = self:GetEnvInfo();
		if tabEnv then 
			self.objMainLight:ImportProperty(tabEnv[1]);
		end
		for _, lightInfo in pairs(tabLightInfo) do 
			local objLight = Game:AddSubLight(self.objMainLight);
			objLight:ImportProperty(lightInfo);
		end
	end;
	--3.初始剧情角色、npc、monster
	for i, v in pairs(tabActor) do 
		if v.dwType ~= 2 then 
			local operator = "CActor";
			if v.dwType == 1 then 
				operator = "CPlayer";
			elseif v.dwType == 3 then
				operator = "CEmpty";
			end
			local func = DStorySystem[operator];
			self[v.name] = func(DStorySystem,v.name, v.skeleton, v.x, v.y, v.z,v.fDir)
			self[v.name]:SetName(v.szRoleName);
		else
			self[v.name] = objMainActor;
			objMainActor:SetLocalPosition(v.x,v.y,v.z);
			objMainActor:SetLocalRotate(0,v.fDir,0);
		end
	end
	--标识状态更改
	self:changeState(true);
end;
--剧情流程分发-----------------------------------------------------------------
--主线剧情消息分发
function StoryBaseSystem:normal_Stage()
	local dwVersion = self:GetVersion();
	if not dwVersion then
		self:hstj_Normal_Stage();
	else
		self:my_Normal_Stage();
	end
end
--副本剧情消息分发
function StoryBaseSystem:scene_Stage()
	local dwVersion = self:GetVersion();
	if not dwVersion then
		self:hstj_Scene_Stage();
	else
		self:my_Scene_Stage();
	end
end
--横扫天界剧情流程-------------------------------------------------------------
function StoryBaseSystem:OnHstjNextStage()
	if not self.isEnding then 
		return 
	end;
	DStorySystem:RemoveTimeActor(self.objMainActor);
	UIStory:ShowInfo(' ', ' ');
	self.dwMiniStage = self.dwMiniStage + 1;
	self.isEnding = false;
	if self.dwMiniStage > #self.tabContent[self.dwStage] then 
		self.dwStage = self.dwStage + 1;
		self.dwMiniStage = 1;
		self.objMainCamera:StopTween();
	end
	print("StoryScript:OnHstjNextStage:", self.dwStage, self.dwMiniStage, self.isEnding)
	local func = self["Stage_"..self.dwStage]
	if not func then 
		--剧情结束
		self.dwStage = 1;
		self.dwMiniStage = 0;
		DStorySystem:Stop();
		self.isEnding = false;
	else
		func(self, self.dwMiniStage);
	end
end
------Hstj-主线剧情流程-------------------------------------------------------------
function StoryBaseSystem:hstj_Normal_Talk()
	local tabCurCfg = self.storyParamConfig[self.dwStage];
	UIStory:ShowInfo(self[tabCurCfg.target]:GetName(), self.tabContent[self.dwStage][self.dwMiniStage]);
end
function StoryBaseSystem:hstj_Normal_Stage()
	print("StoryBaseSystem:hstj_Normal_Stage:", self.dwStage, self.dwMiniStage);
	local dwMiniStage = self.dwMiniStage;
	local objCamera = self.objMainCamera;
	local tabCurCfg = self.storyParamConfig[self.dwStage];
	if dwMiniStage == 1 then 
		local szTarget = tabCurCfg.target
		if tabCurCfg.dwCamType~= 0 then 
			local dwProf = self[szTarget]:GetProf();
			local tabCameraConfig = self.storyCameraConfig[tabCurCfg.dwCamType]
			if dwProf and StoryRoleCameraConfig[dwProf] then 
				tabCameraConfig.distance = StoryRoleCameraConfig[dwProf].distance;
				tabCameraConfig.offset = StoryRoleCameraConfig[dwProf].offset;
			end
			objCamera:UseParam(tabCameraConfig);
		end
		if szTarget then
			objCamera:LookTarget(self[szTarget]);
		end
	end
	local stage = {
		[1] = function() 
			if tabCurCfg.bSmooth == true then  
				StorySmooth.time = tabCurCfg.motionTime;
				objCamera.LRAngle = objCamera.LRAngle + tabCurCfg.motionRange;
				objCamera:Flush(true, StorySmooth)
			end
			self:OnNowStageEnd();
		end;
	}
	self:hstj_Normal_Talk();
	local func = stage[dwMiniStage]
	if not func then 
		self:OnNowStageEnd();
		return 
	end;
	func();
end
------Hstj-副本剧情流程
function StoryBaseSystem:hstj_Scene_Stage()
	local dwMiniStage = self.dwMiniStage;
	local dwSceneTime = 8;
	local objCamera = self.objMainCamera;
	local tabCurCfg = self.storyParamConfig[self.dwStage];
	if dwMiniStage == 1 then 
		local szTarget = tabCurCfg.target
		if tabCurCfg.dwCamType~= 0 then 
			objCamera:UseParam(self.storyCameraConfig[tabCurCfg.dwCamType]);
		end
		if szTarget then
			objCamera:LookTarget(self[szTarget]);
		end
	end
	local stage = {
		[1] = function() 
			if tabCurCfg.bSmooth == true then  
				StorySmooth.time = tabCurCfg.motionTime;
				objCamera.LRAngle = objCamera.LRAngle + tabCurCfg.motionRange;
				objCamera:Flush(true, StorySmooth)
			end
			local paramEnd = {
				-- easetype = EaseType.easeInOutSine;
				time = dwSceneTime;
				oncomplete = function()
				end
			}
			objCamera:UseParam(self.storyCameraConfig[2]);
			objCamera:Flush(true, paramEnd);
			self.objMainActor:TimeEvent(1600, function()
				UIStory:ShowInfo(self[tabCurCfg.target]:GetName(), self.tabContent[self.dwStage][dwMiniStage]);
				self:OnNowStageEnd();
			end)
		end;
	}
	
	local func = stage[dwMiniStage]
	if not func then 
		UIStory:ShowInfo(self[tabCurCfg.target]:GetName(), self.tabContent[self.dwStage][dwMiniStage]);
		self:OnNowStageEnd();
		return 
	end;
	func();
end
--芈月传剧情流程---------------------------------------------------------------
function StoryBaseSystem:OnMyNextStage()
	if not self.isEnding then 
		return 
	end;
	DStorySystem:RemoveTimeActor(self.objMainActor);
	UIStory:ShowInfo(' ', ' ');
	self.dwMiniStage = self.dwMiniStage + 1;
	self.isEnding = false;
	if self.dwMiniStage > #(self.storyConfig.tabContent or {}) then 
		--剧情结束
		self.dwStage = 1;
		self.dwMiniStage = 0;
		DStorySystem:Stop();
		self.isEnding = false;
		return
	end
	local tabContent = self.storyConfig.tabContent[self.dwMiniStage];
	if tabContent.dwNextOpt ~= 0 then 
		self.dwNextOption = tabContent.dwNextOpt;
	end
	if tabContent.dwStage ~= self.dwStage then 
		self.objMainCamera:StopTween();
		self.objPathObject:Stop();
		self:changeState(true);
		self.dwStage = tabContent.dwStage;
	end
	print("StoryScript:OnMyNextStage:", self.dwStage, self.dwMiniStage, self.isEnding)
	local func = self["Stage_"..self.dwStage]
	if not func then 
		--默认剧情处理
		local dwType = self:GetType();
		local typeFunc = {
			[1] = function() self:my_Normal_Stage(); end;
		};
		if typeFunc[dwType] then 
			typeFunc[dwType]();
		end
	else
		func(self, self.dwMiniStage);
	end
end
------miyue-主线剧情流程
function StoryBaseSystem:my_Normal_Talk()
	local tabStage = self.storyConfig.tabStage[self.dwStage];
	local tabContent = self.storyConfig.tabContent[self.dwMiniStage];
	local szName = tabContent.name;
	if szName == "" then 
		szName = self[tabStage.target]:GetName();
	end
	UIStory:ShowInfo(szName, tabContent.info);
end;
function StoryBaseSystem:my_Normal_Stage()
	print("StoryBaseSystem:my_Normal_Stage:", self.dwStage, self.dwMiniStage);
	local dwMiniStage = self.dwMiniStage;
	local objCamera = self.objMainCamera;
	local tabCurCfg = self.storyConfig.tabStage[self.dwStage];
	if self:GetChangeState() == true then 
		local szTarget = tabCurCfg.target
		if tabCurCfg.dwCamType~= 0 then 
			local dwProf = self[szTarget]:GetProf();
			local tabCameraConfig = self.storyConfig.tabCamera[tabCurCfg.dwCamType]
			if dwProf and StoryRoleCameraConfig[dwProf] then 
				tabCameraConfig.distance = StoryRoleCameraConfig[dwProf].distance;
				tabCameraConfig.offset = StoryRoleCameraConfig[dwProf].offset;
			end
			objCamera:UseParam(tabCameraConfig);
		end
		if szTarget then
			objCamera:LookTarget(self[szTarget]);
		end
		self:changeState(false);
	end
	local stage = {
		[1] = function() 
			local tabMiniCfg = self.storyConfig.tabContent[dwMiniStage];
			local tabSmoothCfg = self.storyConfig.tabSmooth[tabMiniCfg.dwSmooth];
			self:my_Smooth(tabSmoothCfg);
			self:OnNowStageEnd();
		end;
		[2] = function() 
			local tabMiniCfg = self.storyConfig.tabContent[dwMiniStage];
			local tabSmoothCfg = self.storyConfig.tabSmooth[tabMiniCfg.dwSmooth];
			self:my_Smooth(tabSmoothCfg);
			self:OnNowStageEnd();
		end;
	}
	self:my_Normal_Talk();
	local func = stage[dwMiniStage]
	if not func then 
		self:OnNowStageEnd();
		return 
	end;
	func();
end;
------miyue-副本剧情流程
function StoryBaseSystem:my_Scene_Stage()
	local dwMiniStage = self.dwMiniStage;
	local dwSceneTime = 8;
	local objCamera = self.objMainCamera;
	local tabCurCfg = self.storyConfig.tabStage[self.dwStage];
	if dwMiniStage == 1 then 
		local szTarget = tabCurCfg.target
		if tabCurCfg.dwCamType~= 0 then 
			objCamera:UseParam(self.storyConfig.tabCamera[tabCurCfg.dwCamType]);
		end
		if szTarget then
			objCamera:LookTarget(self[szTarget]);
		end
	end
	local stage = {
		[1] = function() 
			if tabCurCfg.bSmooth == true then  
				StorySmooth.time = tabCurCfg.motionTime;
				objCamera.LRAngle = objCamera.LRAngle + tabCurCfg.motionRange;
				objCamera:Flush(true, StorySmooth)
			end
			local paramEnd = {
				-- easetype = EaseType.easeInOutSine;
				time = dwSceneTime;
				oncomplete = function()
				end
			}
			objCamera:UseParam(self.storyConfig.tabCamera[2]);
			objCamera:Flush(true, paramEnd);
			self.objMainActor:TimeEvent(dwSceneTime*700, function()
				self:my_Normal_Talk();
				self:OnNowStageEnd();
			end)
		end;
	}
	
	local func = stage[dwMiniStage]
	if not func then 
		UIStory:ShowInfo(self[tabCurCfg.target]:GetName(), self.tabContent[self.dwStage][dwMiniStage]);
		self:OnNowStageEnd();
		return 
	end;
	func();
end;

function StoryBaseSystem:my_Smooth(smoothCfg)
	print("StoryBaseSystem:my_Smooth:", smoothCfg.dwType)
	if not smoothCfg then print("dwMiniStage[", self.dwMiniStage, "] have not smooth..") return end
	if smoothCfg.dwType == 1 then 
		self:my_Tween_Smooth(smoothCfg);
	elseif smoothCfg.dwType == 2 then 
		self:my_Path_Smooth(smoothCfg);
	elseif smoothCfg.dwType == 3 then     --lsy add
		self:my_EnvPath_Smooth(smoothCfg);
	end
end;
function StoryBaseSystem:my_Tween_Smooth(smoothCfg)
	local objCamera = self.objMainCamera;
	StorySmooth.time = smoothCfg.time;
	StorySmooth.LoopType = smoothCfg.LoopType or StorySmooth.LoopType;
	StorySmooth.easetype = smoothCfg.easetype or StorySmooth.easetype;
	objCamera.LRAngle = objCamera.LRAngle + smoothCfg.basicInfo.LRAngle;
	objCamera.UDAngle = objCamera.UDAngle + smoothCfg.basicInfo.UDAngle;
	objCamera.distance = objCamera.distance + smoothCfg.basicInfo.distance;
	objCamera:Flush(true, StorySmooth)
end;
function StoryBaseSystem:my_Path_Smooth(smoothCfg)

	local objCamera = self.objMainCamera;
	self:initCameraPath(smoothCfg);--init
	self.objPathObject.animtarget = objCamera;
	self.objPathObject:Play();
end;

--lsy add 
function StoryBaseSystem:my_EnvPath_Smooth(smoothCfg)
	self:initCameraPath(smoothCfg);--init
	local objCamera = self.objMainCamera;
	objCamera:StopTween();
	objCamera:LookTarget();
	objCamera:SetLocalPosition(0,0,0);
	objCamera:SetLocalRotate(0,0,0);
	self.objPathObject.animtarget = objCamera.parent;
	self.objPathObject:Play();
end
--lsy add
function StoryBaseSystem:initCameraPath(smoothCfg)
	if not self.objPathObject then
		self.objPathObject = Game:AddCameraPath();
	end
	self.objPathObject.localPosition = smoothCfg.localPosition;
	self.objPathObject.controlpoints = smoothCfg.controlpoints;
	self.objPathObject.Orientations = smoothCfg.Orientations;   
	self.objPathObject.interpolation = smoothCfg.interpolation;
	self.objPathObject.animMode = smoothCfg.animMode;
	self.objPathObject.speed = smoothCfg.speed;
	self.objPathObject.isLoop = smoothCfg.isLoop;	
end
