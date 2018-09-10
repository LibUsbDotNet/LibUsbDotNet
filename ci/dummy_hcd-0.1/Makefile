obj-m := dummy_hcd.o
KVERSION := $(shell uname -r)
SVERSION := $(shell uname -r | grep -o "^[^-]*")

all: dummy_hcd.c
	$(MAKE) -C /lib/modules/$(KVERSION)/build M=$(PWD) modules

clean:
	$(MAKE) -C /lib/modules/$(KVERSION)/build M=$(PWD) clean
