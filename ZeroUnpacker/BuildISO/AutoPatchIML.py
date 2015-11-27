import os,struct
def autopatch(img_name , IML_NAME , IMS_NAME):
    _LBA_START = 0
    _LBA_END = 0
    _LBA_LENGTH = 0
    img_size = os.path.getsize(img_name)
    _LBA_LENGTH = img_size / 2048

    iml_file = open(IML_NAME ,"rb+")
    lines = iml_file.readlines()
    for i in xrange(len(lines)):
        line = lines[i]
        if "IMG_BD.BIN" in line:
            a = line.split(" ")
            b = []
            for item in a:
                if item != "":
                    b.append(item)
            print(b)
            _LBA_START = int(b[0])
            _LBA_END = _LBA_START + _LBA_LENGTH
            c = str(int(b[0]) + img_size / 2048 + 1)
            line = line.replace(b[1] ,c)
            lines[i] = line
    iml_file.seek(0)
    iml_file.truncate()
    iml_file.writelines(lines)
    iml_file.close()
    fp = open(IMS_NAME , "rb+")
    fp.seek(0x829d2)
    fp.write(struct.pack("I" , _LBA_LENGTH * 2048))
    fp.write(struct.pack(">I" , _LBA_LENGTH * 2048))
    fp.seek(0x88038)
    fp.write(struct.pack("I" , _LBA_LENGTH * 2048))
    fp.write(struct.pack("I" , 0))
    fp.write(struct.pack("I" , _LBA_LENGTH))
    fp.close()







def main():
    img_name = "IMG_BD.BIN"
    IML_NAME = "ZERO3.iml"
    IMS_NAME = "ZERO3.ims"
    autopatch(img_name , IML_NAME , IMS_NAME)

main()
