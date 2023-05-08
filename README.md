# RAGEParsedData
This tool convert so-called "parseddata" pso/meta (`.#mt`) files into readable XMLs, making it easier to research and analyze. Backwards conversion is available too. However, I wasn't trying to be as close as possible to the way how Rockstar build these files, so layout might be slightly different comparing to original files.
Parsed data format was first seen in Red Dead Redemption 2 era of RAGE (as `rage::sysParsedDataFile*` and other classes). Rockstar use it as a way to store configuration for in-game scripts, they access the data using the `_PARSEDDATA_*` natives. Technically it looks like some flatten XML directly for their engine needs to make streaming and deserializing as fast as possible. These files are stored in `update:/x64/levels/rdr3/script`, inside `parseddata.rpf` and `parseddata_mp.rpf` packfiles.

Here's an example of converted `colter.ymt` file:
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <location center="[-1340.3, 2433.2, 308.2]" id="COLTER" />
</root>
```

And that's how raw and unhashed `colter.ymt` file looks like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<rage__sysParsedDataFile__Data>
  <attributes>
    <Item>
      <name>center</name>
      <valueHash>0xD14811BA</valueHash>
      <contentIndex value="0" />
      <contentType>CONTENT_TYPE_VECTOR</contentType>
      <isEnabled value="true" />
    </Item>
    <Item>
      <name>id</name>
      <valueHash>0x2CF7B29F</valueHash>
      <contentIndex value="0" />
      <contentType>CONTENT_TYPE_STRING</contentType>
      <isEnabled value="true" />
    </Item>
  </attributes>
  <attributeValueStringStore>
    <Item>COLTER</Item>
  </attributeValueStringStore>
  <attributeValueVecStore>
    <Item>
      <x value="-995653222" />
      <y value="1159205683" />
      <z value="1134172570" />
    </Item>
  </attributeValueVecStore>
  <dataNodes>
    <Item>
      <name>root</name>
      <parentIndex value="65535" />
      <childIndex value="1" />
      <siblingIndex value="65535" />
      <previousSiblingIndex value="65535" />
      <attributeStart value="65535" />
      <attributeCount value="0" />
      <isEnabled value="true" />
    </Item>
    <Item>
      <name>location</name>
      <parentIndex value="65535" />
      <childIndex value="65535" />
      <siblingIndex value="65535" />
      <previousSiblingIndex value="65535" />
      <attributeStart value="0" />
      <attributeCount value="2" />
      <isEnabled value="true" />
    </Item>
  </dataNodes>
</rage__sysParsedDataFile__Data>
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
