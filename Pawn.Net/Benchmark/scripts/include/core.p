native managedPrint(len, string[]);
/*native testInt(value);
native setInt(&value);
native setString(value[]);

native testTag(TagData:data);
forward TagTest(TagData:data);

native testFloat(Float:value);

native print_int(value);*/

stock strlen(str[]) {
    new i = 0;
    while(str[i])
        i++;
    return i;
}

stock print(str[]) {
    managedPrint(strlen(str), str);
}
