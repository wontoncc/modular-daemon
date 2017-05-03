modular-daemon
==========

This is a daemon for micro-services written in C# and WPF, targeting .Net 3.5.

![Running in action](https://raw.githubusercontent.com/wontoncc/modular-daemon/master/screenshot.png)

# Usage

```
modular-daemon [--config=[config file]]
```

``` xml
<!-- This is an example config file -->

<config>
  <application>
    <title>[name displayed in the title bar]</title>
  </application>
  <services>
    <service name="[service name]">
      <command>[command to execute]</command>
      <arguments>[command line arguments for the command]</arguments>
      <workingDirectory>[working directory]</workingDirectory>
      <log>[specified log file name]</log>
      <environment>
        <variable name="[variable name]">[value]</variable>
      </environment>
    </service>
  </services>
</config>
```
