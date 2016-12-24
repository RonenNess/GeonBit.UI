import os
import fileter

class AddNewTags(fileter.FilesIterator):
    """
    Iterate over files and replace 'hello' with 'world'.
    """
    def process_file(self, path, dryrun):

        if dryrun:
            return path

        with open(path, "r") as infile:
            buffer = infile.read()
            
        if "Padding" in buffer:
            return path

        buffer = buffer.replace("  </Asset>", """    <Padding Null="true" />
  </Asset>""")

        with open(path, "w") as outfile:
            outfile.write(buffer)

it = AddNewTags()
it.add_pattern("*.xml")
it.process_all()