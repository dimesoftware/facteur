# Contributing

Here are the guidelines we'd like you to follow:

- [Contributing](#contributing)
  - [Code of Conduct](#code-of-conduct)
  - [Semantic Versioning](#semantic-versioning)
  - [Questions, Bugs, Features](#questions-bugs-features)
    - [Questions](#questions)
    - [Bugs](#bugs)
    - [Features](#features)
  - [Issue Submission Guidelines](#issue-submission-guidelines)
  - [Pull request Submission Guidelines](#pull-request-submission-guidelines)
    - [After your pull request is merged](#after-your-pull-request-is-merged)

Make sure to comply with the [code of conduct](CODE_OF_CONDUCT.md).

## Code of Conduct

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v2.0%20adopted-ff69b4.svg)](code_of_conduct.md)

Dime has adopted the [Contributor Covenant](./CODE_OF_CONDUCT.md) as its Code of Conduct, and we expect project participants to adhere to it. Please read the full text so that you can understand what actions will and will not be tolerated.

## Semantic Versioning

Dime follows semantic versioning. We release patch versions for critical bugfixes, minor versions for new features or non-essential changes, and major versions for any breaking changes. When we make breaking changes, we also introduce deprecation warnings in a minor version so that our users learn about the upcoming changes and migrate their code in advance. 

## Questions, Bugs, Features

### Questions

Do not open issues for general support questions as we want to keep GitHub issues for bug reports and feature requests. You've got much better chances of getting your question answered on dedicated support platforms, the best being [Stack Overflow][stackoverflow].

### Bugs

If you find a bug in the source code, you can help us by submitting an issue to our
GitHub repository. Even better, you can submit a pull request with a fix.

Please see the [submission guidelines](#issue-submission-guidelines) below.

### Features

You can request a new feature by submitting an issue to our GitHub repository.

If you would like to implement a new feature then consider what kind of change it is:

* **Major Changes** that you wish to contribute to the project should be discussed first in an issue that clearly outlines the changes and benefits of the feature.
* **Small Changes** can directly be crafted and submitted to the GitHub repository
  as a pull request. See the section about [pull request submission guidelines](#submit-pr).

## Issue Submission Guidelines

Before you submit your issue search the archive, maybe your question was already answered.

If your issue appears to be a bug, and hasn't been reported, open a new issue. Help us to maximize the effort we can spend fixing issues and adding new features, by not reporting duplicate issues. The new issue form contains a number of prompts that you should fill out to
make it easier to understand and categorize the issue.

In general, providing the following information will increase the chances of your issue being dealt with quickly:

- **Overview of the issue**: if an error is being thrown a non-minified stack trace helps
- **Motivation for or use case**: explain why this is a bug for you
- **Version(s)**: is it a regression?
- **Browsers and/or operating system**:is this a problem with all browsers or only specific ones?
- **Reproduce the error**: provide a live example or an unambiguous set of steps.
- **Related issues**: has a similar issue been reported before?
- **Suggest a Fix**: if you can't fix the bug yourself, perhaps you can point to what might be causing the problem (line of code or commit)

## Pull request Submission Guidelines

Before you submit your pull request consider the following guidelines:

- Search GitHub for an open or closed pull request that relates to your submission. You don't want to duplicate effort.
- Create the development environment
- Make your changes in a new git branch:

    ```shell
    git checkout -b my-fix-branch master
    ```

- Create your patch commit, **including appropriate test cases**.
- Follow the coding rules.
- If the changes affect public APIs, change or add relevant documentation.
- Run the test suites, and ensure that all tests pass.
- Commit your changes using a descriptive commit message that follows the commit message convention.

    ```shell
    git commit -a
    ```

  Note: the optional commit `-a` command line option will automatically "add" and "rm" edited files.

- Before creating the pull request, package and run all tests a last time.
- Push your branch to GitHub:

    ```shell
    git push origin my-fix-branch
    ```

- In GitHub, send a pull request to the master branch.

- If you find that the continuous integration tests have failed, look into the logs to find out if your changes caused test failures, the commit message was malformed etc. If you find that the tests failed or times out for unrelated reasons, you can ping a team member so that the build can be restarted.

- If we suggest changes, then:

  - Make the required updates.
  - Re-run the test suite to ensure tests are still passing.
  - Commit your changes to your branch (e.g. `my-fix-branch`).
  - Push the changes to your GitHub repository (this will update your pull request).

    You can also amend the initial commits and force push them to the branch.

    ```shell
    git rebase master -i
    git push origin my-fix-branch -f
    ```

    This is generally easier to follow, but separate commits are useful if the pull request contains iterations that might be interesting to see side-by-side.

That's it! Thank you for your contribution!

### After your pull request is merged

After your pull request is merged, you can safely delete your branch and pull the changes from the main (upstream) repository:

- Delete the remote branch on GitHub either through the GitHub web UI or your local shell as follows:

    ```shell
    git push origin --delete my-fix-branch
    ```

- Check out the master branch:

    ```shell
    git checkout master -f
    ```

- Delete the local branch:

    ```shell
    git branch -D my-fix-branch
    ```

- Update your master with the latest upstream version:

    ```shell
    git pull --ff upstream master
    ```

[stackoverflow]: http://stackoverflow.com
