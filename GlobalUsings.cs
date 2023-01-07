#if UseClonesoft
global using JsonLib = Clonesoft.Json;
global using Clonesoft.Json;
global using Clonesoft.Json.Linq;
#else
global using JsonLib = Newtonsoft.Json;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;
#endif