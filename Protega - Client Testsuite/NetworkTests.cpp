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
		Network_Manager *NetworkTestManager = new Network_Manager(TEST_IP, TEST_PORT, "~", ";",1,1, std::bind(&NetworkTests::ReceiveFunc, this, std::placeholders::_1));
	public:
		
		//tests normal Send/receive by repeating this process two times
		TEST_METHOD(Network_SendReceiveCheck)
		{		
			NetworkTestManager->TestMessage_001();
		}

		void ReceiveFunc(NetworkTelegram NetworkTelegramMessage)
		{
			Assert::AreEqual(NetworkTelegramMessage.iTelegramNumber, 1);
			Assert::AreEqual(NetworkTelegramMessage.lParameters[0], sCheckMessage);
		}
	};
}