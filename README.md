# DbUpdater

A very simple program that reads all the DB scripts (\*.sql) in a folder and runs them on a MySQL/MariaDB Database specified by the connectionstring.

Are there better and faster ways to do this? Probably, but this actually solves an issue for me so I will keep using it.

Before running the scripts, make sure that the database user specified in the connection string exists and has sufficient rights to execute the commands in the scripts.

Both the folder path and connectionstring are supplied via arguments.

## Usage

    DbUpdater.exe [folderPath] [connectionString]

    Example:
    DbUpdater.exe "c:\myscripts" "Server=localhost;Port=3306;Database=TestDb;Uid=testDbUser;Pwd=testDbUserPassword;"

If you are using windows you might want to create a shortcut and then simply run all scripts by double-clicking it.

## Known issues

Since this is not a full SQL parser you might run into issues when the script is not formatted with proper linebreaks and some more advanced commands might not work either. This project is simply a fast way for me to run all my current scripts and update my local databases during development.

# Misc info

I used Windows 10 Home Edition, MS Visual Studio 2019 and .NET Core 5.0 for this project. Testing was performed against a MariaDB database, version 10.3.24.

This project was developed by Marcus Ekberg (Drygast) for Idectus AB.
