


_G.StoryEditorState = {}

function StoryEditorState:Create()
	StorySystem:Create();
end

function StoryEditorState:OnEnterState()
	StorySystem:OnEnterScene(true);
end

function StoryEditorState:OnLeaveState()
	
end

function StoryEditorState:Update()
	StorySystem:Update();
end