# qNetworking
A Unity3D Low-Level API networking system.


#### Network Architecture

##### Setup

There are three separate files for this project. The Master Server (Linux only), the Server (Linux only), and the Game (Win/Mac/Lin). The Server and the Game are technically the same files. The only difference though is if there is a password.pub file present the Game acts as a Server. Once the Master Server starts up, it runs the Server and also generates a highly encrypted password and password.pub file. The Master Server begins listening for connections from a Server and a Game (note: Game connection requires at least1 Server authenticated). The Server reads a configuration file to find out the IP of the Master Server. The Server then sends the password in password.pub to the Master Server. The Master Server receives the password and validates it with the private key. If it is not valid then the connection is dropped. If it is valid then the Server is added to the "serverList" variable in the Master Server.

  

##### Connections

If a Game tries to connect to the Master Server, the Master Server will first verify that there is at least 1 Server in its "serverList". If there is, then the Master Server will permit the client's Game to connect. Once the client's Game connects to the Master Server, the Master Server will assign the Game to a Server that is handling the least amount of players. The Game will be given this Server's IP and will make all computational requests to it. The only time the Game would make a request to a different Server is when the Master Server feels that one Server is starting to have a far greater number of players than other Servers. At this point, the Master Server will start to tell the Games of clients that they should change their assigned Server.

  

##### Reallocation

When one Server is determined to have a far greater number of players than another server (%-wise) then that Server will be sent a "redirection" variable that contains an IP. Any Game that makes a request to a Server with its redirection variable set will be assigned a new Server. Additionally, the Server will get a "redirectionCount" variable that indicates how many players are to be redirected before discarding its redirection variable. It should be noted that when a Game is assigned a Server, the Master Server increments that Server's "playerCount" in the "serverList". It should also decrement the Game's previous Server's "playerCount". This allows for the addition of Servers very easily while the Master Server is running and the game is online. This also allows for Servers that fail or disconnect to redirect Games elsewhere. If a connection times out, the Game will request a new Server from the Master Server. If there are no Servers present, the Master Server will tell all Games to please wait and that it will send the Server IP when there is one present.

  

~Work-in-progress ideas start here~

##### Relaying Client Updates

When a Game requests data from a Server, the Game must also provide data pertaining nearby players. In this scenario, there is an initial player and then there are nearby players. The initial player must share their assigned Server IP with each nearby player. These nearby players must then use this IP, along with an additional unique ID given by the nearby player to send a request to the Server. This request stands until the Server is done processing the data for the initial player. Once it is done, everyone registered with the unique ID, and every future registrants in the next few milliseconds, will get sent the data when the initial player is sent this data as well.

  

##### Spawning Nearby Clients

In this project, the map will be broken up into chunks. When a player enters a new chunk, other players in that chunk should also be spawned in. One thing that has not been worked out, unfortunately, is how to tell which players are in which chunks if there are many players on a single instance of a game.

  

Theoretical idea: The Master Server could keep this data and update it accordingly as players shift between chunks. But this would be bad because you’d be modifying a list of data for hundreds of players constantly. I feel like this would become a big performance issue eventually.

~Work-in-progress ideas end here~
