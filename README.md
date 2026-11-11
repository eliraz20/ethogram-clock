# Ethogram Clock

Compare two observers and locate the precise intervals where their state labels differ.

## Try it

From this folder with the .NET 10 SDK installed:

```sh
dotnet run -- compare study.tsv
```

First row is end time in seconds. Remaining rows give observer, elapsed seconds and state. Both observers start at zero and use the same end time. States are exclusive.

The included study.tsv uses tab-separated fields. Comment lines start with `#`.

The original exercise remains available with `dotnet run -- example.tsv`.

## When to use it

A behavior student codes an observation, then compares a second observer's coding of the same video.

Example result:

```text
40–45 s: Alex = Walk, Bo = Rest
Time agreement: 115/120 s (95.83%)
```

Related course or field workflow: [Animal Behavior Society sampling exercise](https://www.animalbehaviorsociety.org/web/downloads/Sampling%20Animal%20Behavior-SM.pdf).
