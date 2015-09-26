_G.StateMain={}


function StateMain:Create()
	self.systems = {};
	self.uisystems = {};
end

function StateMain:OnEnterState()

end

function StateMain:OnLeaveState()

end

function StateMain:Update()
	for i, objSys in ipairs(self.systems) do
		objSys:Update();
	end
end


function StateMain:AddSystem(sysClass)
	table.insert(self.systems,sysClass);
end

function StateMain:AddUIModule(sysClass)
	table.insert(self.uisystems,sysClass);
end