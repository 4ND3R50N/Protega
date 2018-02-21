#pragma once
#include "stdafx.h"
#include "CppUnitTest.h"
#include "../Protega/Network/Network_Manager.h"
using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{
	//Static vars for network testing
	extern const string TEST_IP =  "62.138.6.50";
	extern const int TEST_PORT = 10000;

	TEST_CLASS(NetworkTests)
	{
	private:
		std::string sCheckMessage = "Hello!";
		
	public:
		
		
	};
}