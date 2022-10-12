#if UseClonesoft
global using JsonLib = Clonesoft.Json;
global using Clonesoft.Json;
#else
global using JsonLib = Newtonsoft.Json;
global using Newtonsoft.Json;
#endif