#if UseClonesoft
global using JsonLib = Clonesoft.Json;
global using Clonesoft.Json;
global using Clonesoft.Json.Linq;
global using Clonesoft.Json.Serialization;
#else
global using JsonLib = Newtonsoft.Json;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;
global using Newtonsoft.Json.Serialization;
#endif