# Asp C# console parking reservation permit app

### learnings
1. run `cd ParkingApp` before running dotnet commands
2. testng run  `dotnet test`
3. hard bugs usig virtual list<ClassSelf> cause phantom classId bug
4. start app from console `dotnet run`
5. optional build executable `dotnet publish -c Release -r win-x64 --self-contained`


## Cheat sheet for git: solution recipes for problems

problems/actions to be done:
- pull a branch: `git pull [remoe] [branch_name]` => `git pull origin main`
- push a branch: `git push [remote] [branch_name]` => `git push origin main`

- create a branch: `git switch -c [new_branch_name]` => `git switch -c develop`
- switch branch: `git switch branch_name`
- delete a branch: `git branch -d branch_name`

- check status: `git status`

# push new work to github/remote flow/steps
0. `git status` to see what pending changes exist
1. `git add .` or `git add -p .` or `gi add filepattern` add all changes or selectively add all changes [n:no,d: no for all changes in this file, y:yes,a: yes for every cange in this file]
2. `git commit -m "some short message about the code changes"` seal your changes 
3. `git push remote branch_name` push the commited changes to your remote repo/github 

- `git diff` see all diffs
- `git diff --staged` see staged diffs

** remote = origin most times
** branch_name = main most times