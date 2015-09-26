

_G.StoryScript_1001={
	bNewSceneState = true;	--是否进入新的场景状态	
}

local CfgPlayer={
	XianNv={
		skeleton="X_XianNv";
		head="T_01";
		body="Y_07";
		weapon="W_07";
		ctrl="Fight";
	};
}

function StoryScript_1001.OnStoryStart(objMainActor, obj)
	print("StoryScript_1001:OnStoryStart:", objMainActor, obj);
	-- local objActor = DStorySystem:CPlayer("主角",CfgPlayer.XianNv,60,30,167,75);
	-- objActor:AddHeadLabel("xxs001", 1, "head")
	-- --DStorySystem:CameraLook(objActor);
	-- local objCamera = Game:GetMainCamera();
	-- objCamera.distance = 45;
	-- objCamera:LookTarget(objActor);

	-- objActor.speed= 3.5;

	-- local obj = DStorySystem:CActor("野猪","A_YeZhu",63,30,163,180);
	-- obj.speed = 1.5;
	-- obj:Move(65,30,167);
	
	-- objActor:Move(65,30,167,function(this)
	-- 	objCamera:SavePoint();
	-- 	objCamera.distance = 10;
	-- 	objCamera:LookTarget(this, true);

	-- 	-- objCamera:SetCameraMove(65, 30, 167);
	-- 	this:Talk("嘿嘿嘿", 5);
	-- 	this:PlayAnim("DaZhao",1.0,function()
	-- 		print("####StoryScript_1001.OnStoryStart####", objCamera.distance)
	-- 		objCamera:BackPoint(true);
	-- 	end)
	-- 	this:PlayEffect("JN/TX_ZhuJue_xiannv_dazhao_FeetPoint")

	-- 	UIManager:PlayEffect("UI/TX_UI_ceshi",0,0,0.3,3);

	-- 	DStorySystem:CPlayer("配角1",CfgPlayer.XianNv,66.5,30,163,-58);
	-- 	DStorySystem:CPlayer("配角2",CfgPlayer.XianNv,59,30,161);

	-- 	this:TimeEvent(2000,function(this)
	-- 		--删除一个角色
	-- 		DStorySystem:DActor("配角1");
	-- 	end);	

	-- 	-- this:TimeEvent(4000,function(this)
	-- 	-- 	--剧情结束
	-- 	-- 	DStorySystem:Stop();
	-- 	-- end);
	-- end);
end
