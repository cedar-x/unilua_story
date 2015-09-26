
_G.CSystem = {}

function CSystem:Create(sysName)
	local obj = {}
	setmetatable(obj,{__index=self});
	obj.listenerList = {};
	obj._inited = false;
	obj._sysName=sysName;
	return obj; 
end


function CSystem:Update()

end

function CSystem:OnEnterScene()

end

function CSystem:SystemName()
	return self._sysName;
end


-- function CSystem:AddListen(sysClass)
-- 	table.insert(sysClass.listenerList,self);
-- end

-- function CSystem:RemoveListen(objListener)
-- 	 for i,listener in pairs(objListener.listenerList) do
-- 	 	if listener == self then
-- 	 		table.remove(objListener.listenerList,i);
-- 	 	end
-- 	 end
-- end

-- function CSystem:DoEvent(szFuncName,...)
-- 	local  args = {...};
-- 	for key,listener in pairs(self.listenerList) do
-- 		local funEvent = listener[szFuncName];
-- 		if	funEvent ~= nil then
-- 			funEvent(listener,unpack(args));
-- 		end
-- 	end
-- end