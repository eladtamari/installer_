Copyright (c) 2014 Qualcomm Technologies, Inc.  All Rights Reserved.
Qualcomm Technologies Proprietary and Confidential.

<sectools>/
| elfsigner.py              (elfsigner launcher command interface)
| sectools.py               (sectools tool launcher command interface)
|
| –- bin/WIN                (Windows binary to perform cryptographic operations)
|
| -- config/                (chipset-specific config files directory)
| -- config/<chipset>/      (preconfigured templates directory)
| -- config/xsd             (xsd for config xml)
|
| -- sectools/features/isc/secimage.py (main Secimage python script)
| -- sectools/features/isc/            (main Secimage core code)
|
| -- resources/data_prov_assets        (assets for signing and encryption)
|
| -- sectools/common/core              (infrastructure)
| -- sectools/common/crypto            (cryptographic services)
| -- sectools/common/data_provisioning (data provision)
| -- sectools/common/parsegen          (image utilities)
| -- sectools/common/utils             (core utilities)

Usage: elfsigner.py [options]

This program signs ELF file with TCG (Trusted Code Group) or generates testsig file with serial number.

Options:
  --version             show program's version number and exit
  -h, --help            show this help message
  -v, --verbose         enable more logging.
  -d, --debug           save debug files and logs.

  Signing ELF file:
    -i <file>, --image_file=<file> path to the image file.
    -r <dir>, --input_dir=<dir> The directory containing multiple image files to sign.

  Generating testsig file with serial number:
    -t <serialnum>, --testsig=<serialnum> 32-bit device serial number such as 0xabcd0123
    

  Common options:
    -o <dir>, --output_dir=<dir> 
                        directory to store output files. DEFAULT: "./output"
    -c, --cass          Use CASS server to sign with production root/key (Requires CASS access)
    -m <tcg>, --tcg_min=<tcg>
                        minimum Trusted Code Group. Override config file value if present.
    -x <tcg>, --tcg_max=<tcg>
                        maximum Trusted Code Group. Override config file value if present.
    -f <tcg>, --tcg_fix=<tcg>
                        fixed Trusted Code Group. Override tcg min and tcg max with the fixed value.
    -p <cass capability>, --capability=<cass capability>
                        CASS capability to select key for signing. Override config file value if present.
    -a, --validate      validate the image. Validation does not support directory (-r).

Examples:
   elfsigner.py -i input/libtest.so
       Signs input/libtest.so and saves signed file to directory: ./output (default)

   elfsigner.py -i input/libtest.so -o signed
       Signs input/libtest.so and saves signed file to directory: ./signed

   elfsigner.py -i libtest.so -c
       Signs libtest.so from CASS server with the default configurations in /config/tcg/tcg_secimage.xml

   elfsigner.py -t 0x1234 -o testsigs/
       Generates testsig-0x1234.so and saves it in directory: ./testsigs
       
Documentation:
80-NM248-4: Sectools: Elfsigner/SecImage Tool User Guide

Support:
sectools.support@qti.qualcomm.com
CASS.support@qualcomm.com
