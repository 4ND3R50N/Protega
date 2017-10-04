#pragma once
#include "../Network/Network_Manager.h"

class ProtegaCore
{
private:
	void ServerAnswer(string sMessage);
public:
	ProtegaCore();
	~ProtegaCore();

	void StartAntihack();
};

