

dofile "Script/Game.lua"
dofile "Script/CSystem.lua"
dofile "Script/GameState/Include.lua"

dofile "Script/StorySystem/Include.lua"


function main(gameObj)
	print("===main..:", gameObj.storyid);
	-- Game:Init(gameObj);
	
	StoryEditorState:Create();

	-- StateMain:Create();
	-- StateMain:AddUIModule(UIStory);
	Game:EnterState(StoryEditorState);
end