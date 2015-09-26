_G.TalkIndex =
{
	[1] = "szNpc";
	[2] = "szPlayer";
}

_G.UIStory = UIBase:UICreate("UIStory");

function UIStory:Create()
	print("UIStory:Create:....")
	self:AddUI("UIStory",UILayer.Bottom,"Story");
	-- self:AddUI("UIStory");
	self:Show();
end
function UIStory:OnLoaded(ui, uiName)
	self.bkContent = ui.bkContent;
	self.szName = ui.bkContent.szName;
	self.szTitle = ui.bkContent.szTitle;
	self.szTalkInfo = ui.bkContent.szTalkInfo;
	self.btnNext = ui.bkContent.btnNext;
	self.btnBkGround = ui.bkGround;
	self.btnBkGround.onClick = function() self:OnBtnNext() end;
	self.btnNext.onClick = function() self:OnBtnNext() end;
	self.btnStop = ui.bkContent.btnStop;
	self.btnStop.onClick = function() self:OnBtnStop() end;
	self:HideInfo();
	if self.tmpTitle then 
		self:ShowTitle(self.tmpTitle);
	end
	if self.tmpActorName and self.tmpTalkInfo then 
		self:ShowInfo(self.tmpActorName, self.tmpTalkInfo);
		self.tmpActorName = nil;
		self.tmpTalkInfo = nil;
	end
end
function UIStory:UnLoaded(uiName)
	self.szName = nil;
	self.szTalkInfo = nil;
	self.btnNext = nil;
	self.btnStop = nil;
end
function UIStory:ShowTitle(szTitle)
	print("UIStory:ShowTitle:", szTitle)
	self.tmpTitle = szTitle;
	if not self:GetUI() then return end;
	self.szTitle.text = self.tmpTitle;
end
function UIStory:ShowInfo(szActorName, szInfo)
	-- print("UIStory:ShowInfo......", szTitle, szInfo)
	self.tmpActorName = szActorName;
	self.tmpTalkInfo = szInfo;
	local ui = self:GetUI();
	if not ui then return end--print("UIStory:ShowInfo:no ui") return end;
	self.bkContent.visible = true;
	self.szName.text = szActorName or "--";
	self.szTalkInfo.text = szInfo or "--";
	local bVisible = true;
	if szInfo == ' ' then 
		bVisible = false;
	end
	self.btnNext.visible = bVisible;
end
function UIStory:HideInfo()
	if not self:GetUI() then return end;
	self.bkContent.visible = false;
end
function UIStory:OnBtnNext()
	local objCamera = Game:GetMainCamera();
	local storyScript = DStorySystem:GetStory();
	storyScript:OnNextStage()
	
	-- objCamera.LRAngle = objCamera.LRAngle + 5;
	-- objCamera:Flush();
end
function UIStory:OnBtnStop()
	-- print("UIStory:OnBtnStop");
	local storyScript = DStorySystem:GetStory();
	storyScript:OnStopStory()
	-- DStorySystem:Stop()
end
