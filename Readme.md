<div align="center">

<h1> Gitbot </h1>
<img alt="GitBot-badge" src="https://img.shields.io/badge/GitBot-blue?logo=github"> 
<img alt="version" src="https://img.shields.io/badge/version-1.0.0-red">
<img alt="Discord-Bot" src="https://img.shields.io/badge/Discord%20Bot-violet?logo=discord">

<p>
<b>GitBot</b> is a Discord Bot that manages repositories locally on the server side, and lets a server owner
control their repositories remotely without having to <i>SSH</i> or remote to their machine. <b>GitBot</b> allows 
users to manage their Git Repositories by talking to the bot.
<p>

</div>


---

### Note

> [!IMPORTANT] 
> This project is still under-development and is being made on my free time, so don't expect consistency and much yet.
> I am farely new to .Net Development, so expect alot of coding mistakes.

---

## Features

**GitBot** has alot of features that makes managing your repostories remotely, as convenient as possible:

 - List all repositories in the server.
 - Switch between repositories.
 - Commit all changes to a repository.
 - Merge, create, switcg and delete branches.
 - Get the status of a repository.
 - Manage files and directories of a repository.
 - Pull and push to remote repository/ies.
	
---

## Installation

To install **GitCord**, you must have *Dot-net SDK 10.0* and *Docker*. With those two installed,
Simply:
1. Clone the repository `git clone https://github.com/patrickcortez/GitCord.git`.
2. Run `docker build -t gitcord` to build gitcord.
3. Then Run **GitCord** `docker run gitcord`

>[!NOTE]
> Make sure you have a folder named `Configurations`
> in the same directory as the *dockerfile*. 
> Which holds your gitcord configurations.

---

## Configuration

Inorder to configure **Gitbot** for your server, Make sure to have:
- repos.json : For listing all allowed repositories to switch to.
- config.json : For placing  all allowed roles and channelids.

Here is an example of repos.json:

```json

{
  "Repos":[
    "C:\\Users\\User\\Test1",
    "C:\\Users\\User\\Repos\\Test2",
    "C:\\Users\\User\\Repos\\Test3"
  ]
}

```

Her is an example of config.json:

```json

{
  "Roles": [
   "123456789",
   "123456798"
  ],
  "GenId": "987654321"
}

```

> [!NOTE]
> Make sure to place it at the same directory as your executable.

---

## License

This project is under **GNU General Public License v3**. See [License](LICENSE.txt)
