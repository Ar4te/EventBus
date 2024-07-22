#备份旧源文件
cp -rf /etc/apt/sources.list.d /etc/apt/sources.list.d.bak
#删除旧源文件
rm -rf /etc/apt/sources.list.d/*
#创建源文件指向华为云源
cat <<'EOF'> /etc/apt/sources.list.d/huawei-cloud.list
deb https://mirrors.huaweicloud.com/debian/ bookworm main contrib non-free
deb https://mirrors.huaweicloud.com/debian/ bookworm-updates main contrib non-free
deb https://mirrors.huaweicloud.com/debian/ bookworm-backports main contrib non-free
deb https://mirrors.huaweicloud.com/debian-security/ bookworm-security main contrib non-free
EOF

cat <<'EOF'> fix_skiasharp_broke.sh
#!/bin/bash

# 更新包依赖树，确保最新
apt-get update -y

# 安装 libgdiplus 库，这是 System.Drawing.Common 在 Linux 上的依赖
apt-get install -y libgdiplus

# 清理 apt 缓存，减少镜像大小
apt-get clean

# 检查是否存在 /usr/lib/gdiplus.dll 文件，如果不存在，则创建一个指向 libgdiplus.so 的符号链接
# 这是为了兼容某些依赖 gdiplus.dll 的应用
if [ ! -f /usr/lib/gdiplus.dll ]; then
    ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
fi
EOF

sh fix_skiasharp_broke.sh