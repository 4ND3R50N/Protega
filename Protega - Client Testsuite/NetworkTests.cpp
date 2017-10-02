#pragma once
#include "stdafx.h"
#include "CppUnitTest.h"
#include "../Protega/Network/Network_Manager.h"
using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ProtegaClientTestsuite
{
	//Static vars for network testing
	extern const string TEST_IP =  "62.138.6.50";
	extern const string TEST_PORT = "13001";

	TEST_CLASS(NetworkTests)
	{
	private:
		string sCheckMessage = "Hello!面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面面";
		Network_Manager *NetworkTestManager = new Network_Manager(TEST_IP, TEST_PORT, std::bind(&NetworkTests::ReceiveFunc, this, std::placeholders::_1));
	public:
		
		//tests normal Send/receive by repeating this process two times
		TEST_METHOD(SendReceiveCheck)
		{		
			NetworkTestManager->TestMessage_001();
		}

		void ReceiveFunc(string sMessage)
		{
			Assert::AreEqual(sCheckMessage, sMessage);
		}


	};
}