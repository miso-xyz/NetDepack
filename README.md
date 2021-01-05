# NetDepack
(currently scrapped, not working) - Unpacker for Mageland29's NETPacker

# Usefull info
- Made under vs2017 with .NET Framework 4.6 (can't use anything under 4.5.2 due to ```Marshal.SizeOf``` Instructions)
- The program loads the packed application to get its Handle, the packed application is directly frozen when started, meaning it will not be able to do anything. (used code of ZrCulillo's JIT Freezer)

# Known Problem(s)
- Failing to decrypt (OverflowException)

# Credits:
- ZrCulillo - https://github.com/ZrCulillo/JIT-Freezer
- 0xd4d - https://github.com/dnlib/dnlib
