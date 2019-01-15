#===============================================================================
#
# Copyright (c) 2014 Qualcomm Technologies, Inc. All Rights Reserved.
# Qualcomm Technologies Proprietary and Confidential.
#
#===============================================================================

'''
Created on May 20, 2014

@author: forrestm
'''

from collections import namedtuple

TEMP_SOURCE = None
TEMP_SUBSOURCE = None

Key_Prov_Entry = namedtuple('Key_Prov_Entry','algorithm_name key_name source subsource key')

class KeyProvisioner:
    '''
    Maintains a database that maps algorithm names to their respective keys


    database:
         list(Key_Prov_Entry(algorithm,key_name,source,subsource,actual_key))

    #TODO:
    determine if subsource is necessary
    determine if modifications will be made at times other than construction (I'm thinking no)
    determine how to initially set default source/subsource.
    '''

    database = []
    default_source = TEMP_SOURCE
    default_subsource = TEMP_SUBSOURCE
    initialized = False

    @staticmethod
    def getKey(algorithm_name,key_name,source = TEMP_SOURCE, subsource = TEMP_SUBSOURCE):
        ''' returns the requested key
        raises runtime error is any of the params aren't found
        :param algorithm_name: name of the encryption algorithm to retrieve key for. Ex: "unified" "ssd"
        :type algorithm_name: str
        :param key_name: name of the key to be retrieved. Ex: "RSA-2048" "L1" "L2"
        :type key_name: str
        :param source:  name of the source for the key to be pulled from. Ex: "Server" "Config"
        :type source: str
        :param subsource: name of the subsource for the key to be pulled from. Ex: "Test" "Release" "Internal" "Provisioning"
        :type subsource: str
        :raises: RuntimeError
        '''

        for entry in KeyProvisioner.database:
            if entry.algorithm_name == algorithm_name and entry.key_name == key_name and entry.source == source and entry.subsource == subsource:
                return entry.key
        raise RuntimeError("given algo, key, source, and subsource were not found in keyprov database")
        return None

    # TODO: determine what exactly this should return
    # a list of key binaries? dictionary that maps the name to the binary?
    # named tuple of the keys?

    @staticmethod
    def getAllKeys(algorithm_name,source = TEMP_SOURCE, subsource = TEMP_SUBSOURCE):
        ''' returns the list of requested keys
        raises runtime error is any of the params aren't found
        :param algorithm_name: name of the encryption algorithm to retrieve key for. Ex: "unified" "ssd"
        :type algorithm_name: str
        :param source:  name of the source for the key to be pulled from. Ex: "Server" "Config"
        :type source: str
        :param subsource: name of the subsource for the key to be pulled from. Ex: "Test" "Release" "Internal" "Provisioning"
        :type subsource: str
        :raises: RuntimeError
        '''
        matching_entries = []
        for entry in KeyProvisioner.database:
            if entry.algorithm_name == algorithm_name and entry.source == source and entry.subsource == subsource:
                matching_entries[entry.key_name] = entry.key
        if len(matching_entries) == 0:
            raise RuntimeError("given algo, source, and subsource were not found in keyprov database")
        return matching_entries

    @staticmethod
    def setDefaultSource(source):
        ''' specifies default source for keys to be pulled from if source isn't specified on retrieval
        :param source: Name of the default source
        :type source: str
        '''
        KeyProvisioner.default_source = source

    @staticmethod
    def setDefaultSubSource(subsource):
        '''specifies the default subsource for keys to be pulled from if source isn't specified on retrieval
        :param source: Name of the default subsource
        :type source: str

        '''
        KeyProvisioner.default_subsource = subsource

    #TODO: determine how we actually want to deal with config file
    #fight now, this takes in the config from isc/encryption_service/unified/encryption_parameters
    #and queries it for unified_encrpytion and L1/2/3 keys


    @staticmethod
    def init(config):
        ''' initializes the key database from the encryption_parameters config
        :param config: config list(?) for encryption_parameters
        :type config: ???
        '''

        #TODO: decide if we want 'unified' and 'L1'/2/3 hardcoded in here or put into some defines somewhere

        if(KeyProvisioner.initialized == True):
            raise RuntimeError("Key provisioner has already been initialized")
        KeyProvisioner.unitialized = True

        if config.unified_encryption.use_file_interface==True:
            try:
                with open(config.unified_encryption.L1_encryption_key, 'r') as f:
                    KEY=f.read()
                    KeyProvisioner._addKey('unified','L1',TEMP_SOURCE,TEMP_SUBSOURCE,KEY)
            except:
                raise RuntimeError("Cannot read from L1 key file")

            try:
                with open(config.unified_encryption.L2_encryption_key, 'r') as f:
                    KEY=f.read()
                    KeyProvisioner._addKey('unified','L2',TEMP_SOURCE,TEMP_SUBSOURCE,KEY)
            except:
                raise RuntimeError("Cannot read from L2 key file")

            try:
                with open(config.unified_encryption.L3_encryption_key, 'r') as f:
                    KEY=f.read()
                    KeyProvisioner._addKey('unified','L3',TEMP_SOURCE,TEMP_SUBSOURCE,KEY)
            except:
                raise RuntimeError("Cannot read from L3 key file")
        print KeyProvisioner.database

        #TODO: ssd encryption???

    @staticmethod
    def _addKey(algorithm_name, key_name, source, subsource, key):
        KeyProvisioner.database.append(Key_Prov_Entry(algorithm_name,key_name,TEMP_SOURCE,TEMP_SUBSOURCE,key))

