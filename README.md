# MQContract
[![.NET-Test-8x](https://github.com/roger-castaldo/MQContract/actions/workflows/unittests8x.yml/badge.svg)](https://github.com/roger-castaldo/MQContract/actions/workflows/unittests8x.yml)
[![CodeQL](https://github.com/roger-castaldo/MQContract/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/roger-castaldo/MQContract/actions/workflows/github-code-scanning/codeql)

The idea behind MQContract is to wrap the interactions with multiple MQ server types (ie KubeMQ, NATS.io, Kafka ...) in a simple and easy to use interface.  
This is done through defining Messages (classes) and tagging them appropriately as necessary, then using those to interact with a ContractConnection.  Using this concept 
you can create a general library that contains all your Message classes (contract definitions) that are expected to be transmitted and recieved within a system.
In addition to this, there is also the ability to specify versions for a given message type and implementing converters for original message types to the new 
types.  This is in line with the idea of not breaking a contract within a system by updating a listener and giving time to other developers/teams to update 
their systems to supply the new version of the message.  By default all message body's are json encoded and unencrypted, all of which can be overridden on a 
global level or on a per message type level through implementation of the appropriate interfaces.

* [Abstractions](/Abstractions/Readme.md)
* [Core](/Core/Readme.md)
* Connectors
	* [ActiveMQ](/Connectors/ActiveMQ/Readme.md)
	* [HiveMQ](/Connectors/HiveMQ/Readme.md)
	* [InMemory](/Connectors/InMemory/Readme.md)
	* [Kafka](/Connectors/Kafka/Readme.md)
	* [KubeMQ](/Connectors/KubeMQ/Readme.md)
	* [Nats.io](/Connectors/NATS/Readme.md)
	* [RabbitMQ](/Connectors/RabbitMQ/Readme.md)
	* [Redis](/Connectors/Redis/Readme.md)