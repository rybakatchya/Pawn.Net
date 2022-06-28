#include <amx.h>
#include <amx_loader.h>
#include <cassert>
#include <cstdio>
#include <fstream>
#include <vector>

bool read_all(const char* path, std::vector<uint8_t>& data)
{
  data.clear();
  std::ifstream is(path, std::ios::binary);
  if (!is.good() || !is.is_open())
    return false;
  is.seekg(0, std::ifstream::end);
  data.resize((size_t)is.tellg());
  is.seekg(0, std::ifstream::beg);
  is.read(reinterpret_cast<char*>(data.data()), (std::streamsize)data.size());
  if (!is.good() || !is.is_open())
    return false;
  return true;
}

bool extract_string(std::string& out, amx* amx, amx_cell va)
{
  out.clear();
  while (true)
  {
    amx_cell cell{};
    if (AMX_SUCCESS != amx_cell_read(amx, va, &cell))
      return false;
    if (!cell)
      break;
    out += (char)cell;
    va += sizeof(amx_cell);
  }
  return true;
}

int main(int argc, char** argv)
{
  if (argc < 2)
  {
    fprintf(stderr, "No input given.\nUsage: %s <amx file>\n", argv[0]);
    return -1;
  }
  const auto loader = amx_loader_alloc();
  assert(loader);

  std::vector<uint8_t> file;
  if (!read_all(argv[1], file))
  {
    fprintf(stderr, "File %s unreadable.\n", argv[1]);
    return -2;
  }

  amx_loader_register_native(
    loader,
    "my_print",
    [](struct amx_loader* loader, struct amx* amx, void* userdata) -> int {
      const auto stk = amx_register_read(amx, AMX_STK);
      amx_cell str_ptr{};
      auto status = amx_cell_read(amx, stk + sizeof(amx_cell), &str_ptr);
      if (status != AMX_SUCCESS)
        return 0;
      std::string text;
      if (!extract_string(text, amx, str_ptr))
        return 0;
      printf("%s\n", text.c_str());
      amx_register_write(amx, AMX_PRI, 0);
      return 1;
    });

  amx_cell main{};
  auto status = amx_loader_load(loader, file.data(), file.size(), &main);
  if (status != AMX_SUCCESS)
  {
    fprintf(stderr, "File %s cannot be loaded: %X.\n", argv[1], status);
    return -3;
  }

  const auto my_amx = amx_loader_get_amx(loader);

  if (main)
    status = amx_call(my_amx, main);
  else
    printf("No main function...\n");

  if (status != AMX_SUCCESS)
    printf("main failed with %d\n", status);

  amx_loader_free(loader);
  return 0;
}
