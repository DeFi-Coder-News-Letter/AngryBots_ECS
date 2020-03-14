using System.Collections.Generic;

public static class MainPlayerObjectRegistry
{
    // keeps a list of all the players
    static List<MainPlayerObject> players = new List<MainPlayerObject>();

    // create a player for a connection
    // note: connection can be null
    static MainPlayerObject CreatePlayer(BoltConnection connection)
    {
        MainPlayerObject player;

        // create a new player object, assign the connection property
        // of the object to the connection was passed in
        player = new MainPlayerObject();
        player.connection = connection;

        // if we have a connection, assign this player
        // as the user data for the connection so that we
        // always have an easy way to get the player object
        // for a connection
        if (player.connection != null)
        {
            player.connection.UserData = player;
        }

        // add to list of all players
        players.Add(player);

        return player;
    }

    // this simply returns the 'players' list cast to
    // an IEnumerable<T> so that we hide the ability
    // to modify the player list from the outside.
    public static IEnumerable<MainPlayerObject> AllPlayers
    {
        get { return players; }
    }

    // finds the server player by checking the
    // .IsServer property for every player object.
    public static MainPlayerObject ServerPlayer
    {
        get { return players.Find(player => player.IsServer); }
    }

    // utility function which creates a server player
    public static MainPlayerObject CreateServerPlayer()
    {
        return CreatePlayer(null);
    }

    // utility that creates a client player object.
    public static MainPlayerObject CreateClientPlayer(BoltConnection connection)
    {
        return CreatePlayer(connection);
    }

    // utility function which lets us pass in a
    // BoltConnection object (even a null) and have
    // it return the proper player object for it.
    public static MainPlayerObject GetMainPlayer(BoltConnection connection)
    {
        if (connection == null)
        {
            return ServerPlayer;
        }

        return (MainPlayerObject)connection.UserData;
    }
}