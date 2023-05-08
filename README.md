# RAGEParsedData
This tool convert so-called "parseddata" pso/meta (`.#mt`) files into readable XMLs, making it easier to research and analyze. Backwards conversion is available too. However, I wasn't trying to be as close as possible to the way how Rockstar build these files, so layout might be slightly different comparing to original files.
Parsed data format is stored was first seen in Red Dead Redemption 2 era RAGE (as `rage::sysParsedDataFile*` classes). Rockstar use it as a way to store configuration for in-game scripts, they access the data using the `_PARSEDDATA_*` natives. Technically it looks like some flatten XML directly for their engine needs to make streaming and deserializing as fast as possible. These files are stored in `update:/x64/levels/rdr3/script`, inside `parseddata.rpf` and `parseddata_mp.rpf` packfiles.

Here's an example of converted `colter.ymt` file:
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <location center="[-1340.3, 2433.2, 308.2]" id="COLTER" />
</root>
```

And that's how raw `colter.ymt` file looks like:
```xml
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<UNK_TYPE_0xDE396FE2>
  <UNK_MEMBER_0x749DBB9E>
    <Item>
      <name>0x9A6411E7</name>
      <UNK_MEMBER_0x817ED7A4>0xD14811BA</UNK_MEMBER_0x817ED7A4>
      <UNK_MEMBER_0x93BAB68E value="0"/>
      <UNK_MEMBER_0x2A8E416F>0x7F1EBDFC</UNK_MEMBER_0x2A8E416F>
      <UNK_MEMBER_0x31A7C9F6 value="true"/>
    </Item>
    <Item>
      <name>0x1B60404D</name>
      <UNK_MEMBER_0x817ED7A4>0x2CF7B29F</UNK_MEMBER_0x817ED7A4>
      <UNK_MEMBER_0x93BAB68E value="0"/>
      <UNK_MEMBER_0x2A8E416F>0x665E1B60</UNK_MEMBER_0x2A8E416F>
      <UNK_MEMBER_0x31A7C9F6 value="true"/>
    </Item>
  </UNK_MEMBER_0x749DBB9E>
  <UNK_MEMBER_0x5C2411E5>
    <Item>COLTER</Item>
  </UNK_MEMBER_0x5C2411E5>
  <UNK_MEMBER_0xD54B8545>
    <Item>
      <x value="-995653222"/>
      <y value="1159205683"/>
      <z value="1134172570"/>
    </Item>
  </UNK_MEMBER_0xD54B8545>
  <UNK_MEMBER_0x2260B8BE>
    <Item>
      <name>0xC090AFAD</name>
      <parentIndex value="65535"/>
      <UNK_MEMBER_0x01296AED value="1"/>
      <UNK_MEMBER_0x881BF649 value="65535"/>
      <UNK_MEMBER_0xF33047D9 value="65535"/>
      <UNK_MEMBER_0x03341D59 value="65535"/>
      <UNK_MEMBER_0x79661755 value="0"/>
      <UNK_MEMBER_0x31A7C9F6 value="true"/>
    </Item>
    <Item>
      <name>0xFBC129D2</name>
      <parentIndex value="65535"/>
      <UNK_MEMBER_0x01296AED value="65535"/>
      <UNK_MEMBER_0x881BF649 value="65535"/>
      <UNK_MEMBER_0xF33047D9 value="65535"/>
      <UNK_MEMBER_0x03341D59 value="0"/>
      <UNK_MEMBER_0x79661755 value="2"/>
      <UNK_MEMBER_0x31A7C9F6 value="true"/>
    </Item>
  </UNK_MEMBER_0x2260B8BE>
</UNK_TYPE_0xDE396FE2>
```

## How to use
- Clone this repo and build the solution using Visual Studio.
- Extract "parseddata" files as XML using OpenIV (unfortunately CodeX can't open these files properly as for now).
- It is highly recommended to put a [list of known strings](https://github.com/cpmodding/Codex.Games.RDR2.strings) into `strings.txt`.
- Run the tool using CLI and pass all required arguments (`RAGEParsedData [command] [input] [output]`).
- Convert RAGE parsed data to readable XML: `RAGEParsedData.exe convert "C:\parseddata" "C:\converted"`
- Convert readable XML to RAGE parsed data: `RAGEParsedData.exe parse "C:\converted" "C:\parseddata"`

## License
MIT.
